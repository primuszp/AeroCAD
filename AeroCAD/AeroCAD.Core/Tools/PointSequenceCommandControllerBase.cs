using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    /// <summary>
    /// SDK-facing base class for commands that collect a sequence of world points.
    /// It centralizes point submission, pointer-preview updates, undo, completion,
    /// cancellation, and AddEntityCommand routing while leaving domain-specific
    /// keywords and entity construction to subclasses.
    /// </summary>
    public abstract class PointSequenceCommandControllerBase : CommandControllerBase
    {
        private readonly List<Point> points = new List<Point>();

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override CommandStep InitialStep => FirstPointStep;

        protected IReadOnlyList<Point> Points => points.AsReadOnly();

        protected abstract CommandStep FirstPointStep { get; }

        protected abstract CommandStep NextPointStep { get; }

        protected virtual int MinimumPointCount => 2;

        protected virtual string MinimumPointCountMessage => string.Format(CultureInfo.InvariantCulture, "At least {0} points are required.", MinimumPointCount);

        protected virtual string CanceledMessage => $"{CommandName} canceled.";

        protected virtual string CreatedMessage => $"{CommandName} created.";

        protected virtual string NothingToUndoMessage => "Nothing to undo.";

        protected virtual string FirstPointRemovedMessage => "First point removed.";

        protected virtual string LastPointRemovedMessage => "Last point removed.";

        public override void OnActivated(IInteractiveCommandHost host)
        {
            ClearPoints();
            ClearRubberPreview(host);
            OnSequenceActivated(host);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (points.Count == 0)
                return;

            var previewPoint = host.ResolveFinalPoint(points[points.Count - 1], rawPoint);
            SetSequencePreview(host, previewPoint);
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            var basePoint = GetCurrentBasePoint();
            return SubmitPoint(host, host.ResolveFinalPoint(basePoint, rawPoint), logInput: true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (TrySubmitCustomToken(host, token, out var customResult))
                return customResult;

            if (!host.TryResolvePointInput(token, GetCurrentBasePoint(), out var point))
                return InteractiveCommandResult.Unhandled();

            return SubmitPoint(host, point, logInput: true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            if (TryCompleteCustom(host, out var customResult))
                return customResult;

            if (points.Count == 0)
                return EndSequence(host, CanceledMessage);

            if (points.Count < MinimumPointCount)
            {
                LogMessage(host, MinimumPointCountMessage);
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            return CompleteSequence(host);
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return EndSequence(host, CanceledMessage);
        }

        protected virtual void OnSequenceActivated(IInteractiveCommandHost host)
        {
        }

        protected virtual bool TrySubmitCustomToken(IInteractiveCommandHost host, CommandInputToken token, out InteractiveCommandResult result)
        {
            result = null;
            return false;
        }

        protected virtual bool TryCompleteCustom(IInteractiveCommandHost host, out InteractiveCommandResult result)
        {
            result = null;
            return false;
        }

        protected virtual void OnPointAdded(Point point)
        {
        }

        protected virtual void OnPointRemoved(Point point)
        {
        }

        protected virtual GripPreview CreatePreview(Point? previewPoint)
        {
            return GripPreview.Empty;
        }

        protected abstract Entity CreateEntity();

        protected virtual Layer ResolveActiveLayer(IInteractiveCommandHost host)
        {
            var editorState = host?.ToolService?.GetService<IEditorStateService>();
            if (editorState?.ActiveLayer != null)
                return editorState.ActiveLayer;

            var document = host?.ToolService?.GetService<ICadDocumentService>();
            return document?.Layers?.Count > 0 ? document.Layers[0] : null;
        }

        protected InteractiveCommandResult SubmitPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            points.Add(point);
            OnPointAdded(point);

            if (logInput)
                LogInput(host, InteractiveCommandToolBase.FormatPoint(point));

            SetSequencePreview(host);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        protected InteractiveCommandResult UndoLastPoint(IInteractiveCommandHost host)
        {
            LogInput(host, "Undo");

            if (points.Count == 0)
            {
                LogMessage(host, NothingToUndoMessage);
                ClearRubberPreview(host);
                return InteractiveCommandResult.MoveToStep(FirstPointStep);
            }

            var removed = points[points.Count - 1];
            points.RemoveAt(points.Count - 1);
            OnPointRemoved(removed);

            if (points.Count == 0)
            {
                LogMessage(host, FirstPointRemovedMessage);
                ClearRubberPreview(host);
                return InteractiveCommandResult.MoveToStep(FirstPointStep);
            }

            LogMessage(host, LastPointRemovedMessage);
            SetSequencePreview(host);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        protected InteractiveCommandResult CompleteSequence(IInteractiveCommandHost host, string message = null)
        {
            var entity = CreateEntity();
            if (entity == null)
                return InteractiveCommandResult.MoveToStep(points.Count == 0 ? FirstPointStep : NextPointStep);

            if (!AddEntity(host, entity))
                return InteractiveCommandResult.MoveToStep(points.Count == 0 ? FirstPointStep : NextPointStep);

            return EndSequence(host, message ?? CreatedMessage);
        }

        protected InteractiveCommandResult EndSequence(IInteractiveCommandHost host, string message)
        {
            ClearPoints();
            return EndCommand(host, message);
        }

        protected void SetSequencePreview(IInteractiveCommandHost host, Point? previewPoint = null)
        {
            var rubberObject = host?.ToolService?.Viewport?.GetRubberObject();
            if (rubberObject == null)
                return;

            rubberObject.Preview = CreatePreview(previewPoint);
        }

        protected void LogInput(IInteractiveCommandHost host, string input)
        {
            host?.ToolService?.GetService<ICommandFeedbackService>()?.LogInput(input);
        }

        protected void LogMessage(IInteractiveCommandHost host, string message)
        {
            host?.ToolService?.GetService<ICommandFeedbackService>()?.LogMessage(message);
        }

        private Point? GetCurrentBasePoint()
        {
            return points.Count > 0 ? points[points.Count - 1] : (Point?)null;
        }

        private bool AddEntity(IInteractiveCommandHost host, Entity entity)
        {
            var layer = ResolveActiveLayer(host);
            var document = host?.ToolService?.GetService<ICadDocumentService>();
            if (layer == null || document == null)
            {
                LogMessage(host, "No active layer is available.");
                return false;
            }

            var command = new AddEntityCommand(document, layer.Id, entity);
            var undoRedo = host.ToolService.GetService<IUndoRedoService>();
            if (undoRedo != null)
                undoRedo.Execute(command);
            else
                command.Execute();

            return true;
        }

        private void ClearPoints()
        {
            while (points.Count > 0)
            {
                var removed = points[points.Count - 1];
                points.RemoveAt(points.Count - 1);
                OnPointRemoved(removed);
            }
        }
    }
}
