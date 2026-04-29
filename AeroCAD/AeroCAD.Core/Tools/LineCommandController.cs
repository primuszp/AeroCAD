using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class LineCommandController : CommandControllerBase
    {
        private static readonly CommandKeywordOption CloseKeyword =
            new CommandKeywordOption("Close", new[] { "C" }, "Close the line back to the first point.");

        private static readonly CommandKeywordOption UndoKeyword =
            new CommandKeywordOption("Undo", new[] { "U" }, "Remove the last segment.");

        private static readonly CommandStep FirstPointStep =
            new CommandStep("FirstPoint", "Specify first point:");

        private static readonly CommandStep NextPointStep =
            new CommandStep("NextPoint", "Specify next point:", keywords: new[] { CloseKeyword, UndoKeyword });

        private readonly System.Func<Layer> activeLayerResolver;
        private readonly LineInteractiveShapeSession session = new LineInteractiveShapeSession();

        public LineCommandController()
            : this(null)
        {
        }

        public LineCommandController(System.Func<Layer> activeLayerResolver)
        {
            this.activeLayerResolver = activeLayerResolver;
        }

        public override string CommandName => "LINE";

        public override CommandStep InitialStep => FirstPointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            session.Reset();
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (session.Drawing)
                host.ToolService.Viewport.GetRubberObject().SetMove(host.ResolveFinalPoint(session.StartPoint, rawPoint));
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = session.Drawing
                ? host.ResolveFinalPoint(session.StartPoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            CommandKeywordOption keyword;
            if (session.Drawing && TryResolveKeyword(host, token, out keyword))
            {
                if (keyword == CloseKeyword)
                    return CloseLine(host);

                if (keyword == UndoKeyword)
                    return UndoLastSegment(host);
            }

            Point point;
            if (!host.TryResolvePointInput(token, session.Drawing ? session.StartPoint : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Cancel(host, "LINE ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Cancel(host, "LINE canceled.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            var feedback = host.ToolService.GetService<ICommandFeedbackService>();
            if (logInput)
                feedback?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!session.Drawing)
            {
                session.Begin(point);
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.CurrentStyle = RubberStyle.Line;
                rbo.SetStart(session.StartPoint);
                return InteractiveCommandResult.MoveToStep(NextPointStep);
            }

            CreateLineSegment(host, session.StartPoint, point);
            session.AddVertex(point);
            host.ToolService.Viewport.GetRubberObject().SetStart(session.StartPoint);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private void CreateLineSegment(IInteractiveCommandHost host, Point from, Point to)
        {
            var layer = ResolveActiveLayer(host);
            if (layer == null)
                return;

            var line = new Line(from, to);
            var document = host.ToolService.GetService<ICadDocumentService>();
            var cmd = new AddEntityCommand(document, layer.Id, line);
            host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
            session.AddSegment(line);
        }

        private Layer ResolveActiveLayer(IInteractiveCommandHost host)
        {
            if (activeLayerResolver != null)
                return activeLayerResolver();

            var editorState = host?.ToolService?.GetService<IEditorStateService>();
            if (editorState?.ActiveLayer != null)
                return editorState.ActiveLayer;

            var document = host?.ToolService?.GetService<ICadDocumentService>();
            return document?.Layers?.Count > 0 ? document.Layers[0] : null;
        }

        private InteractiveCommandResult CloseLine(IInteractiveCommandHost host)
        {
            if (!session.CanClose())
                return InteractiveCommandResult.MoveToStep(NextPointStep);

            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput("Close");
            CreateLineSegment(host, session.StartPoint, session.FirstPoint);
            return Finish(host, "LINE ended.");
        }

        private InteractiveCommandResult UndoLastSegment(IInteractiveCommandHost host)
        {
            if (!session.CanUndo())
                return InteractiveCommandResult.MoveToStep(NextPointStep);

            var document = host.ToolService.GetService<ICadDocumentService>();
            var undoRedo = host.ToolService.GetService<IUndoRedoService>();
            var lastSegment = session.CreatedSegments.Count > 0 ? session.CreatedSegments[session.CreatedSegments.Count - 1] : null;
            var shouldRemoveDocumentEntity = lastSegment != null;
            if (shouldRemoveDocumentEntity)
                undoRedo?.Execute(new RemoveEntitiesCommand(document, new Entity[] { lastSegment }, "Undo Line Segment"));

            session.UndoLast(out _);
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput("Undo");

            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            if (!session.Drawing)
            {
                rubberObject.Cancel();
                rubberObject.ClearPreview();
                rubberObject.SnapPoint = null;
                rubberObject.InvalidateVisual();
                return InteractiveCommandResult.MoveToStep(FirstPointStep);
            }

            rubberObject.SetStart(session.StartPoint);
            return InteractiveCommandResult.MoveToStep(NextPointStep);
        }

        private InteractiveCommandResult Cancel(IInteractiveCommandHost host, string message)
        {
            return Finish(host, message);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            session.Reset();
            return EndCommand(host, message);
        }
    }
}

