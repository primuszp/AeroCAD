using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Selection;
using Primusz.AeroCAD.Core.Spatial;
using Primusz.AeroCAD.Core.Tools;

namespace Primusz.AeroCAD.Core.Plugins
{
    public sealed class InteractiveCommandContext
    {
        public InteractiveCommandContext(IInteractiveCommandHost host)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public IInteractiveCommandHost Host { get; }

        public IToolService ToolService => Host.ToolService;

        public IViewport Viewport => ToolService?.Viewport;

        public CommandStep CurrentStep => Host.CurrentStep;

        public ICadDocumentService Document => ToolService?.GetService<ICadDocumentService>();

        public IUndoRedoService UndoRedo => ToolService?.GetService<IUndoRedoService>();

        public ICommandFeedbackService Feedback => ToolService?.GetService<ICommandFeedbackService>();

        public ISelectionManager Selection => ToolService?.GetService<ISelectionManager>();

        public IEditorStateService EditorState => ToolService?.GetService<IEditorStateService>();

        public Layer ActiveLayer => EditorState?.ActiveLayer ?? Document?.Layers?.FirstOrDefault();

        public T GetService<T>() where T : class
        {
            return ToolService?.GetService<T>();
        }

        public bool TryResolvePoint(CommandInputToken token, Point? basePoint, out Point point)
        {
            return Host.TryResolvePointInput(token, basePoint, out point);
        }

        public bool TryResolveScalar(CommandInputToken token, out double scalar)
        {
            return Host.TryResolveScalarInput(token, out scalar);
        }

        public bool TryResolveDistance(CommandInputToken token, Point? basePoint, out double distance)
        {
            if (TryResolveScalar(token, out distance))
                return true;

            if (basePoint.HasValue && TryResolvePoint(token, basePoint, out var point))
            {
                distance = (point - basePoint.Value).Length;
                return true;
            }

            distance = 0d;
            return false;
        }

        public double ResolveDistance(Point basePoint, Point point)
        {
            return (point - basePoint).Length;
        }

        public Point ResolveFinalPoint(Point? basePoint, Point rawPoint)
        {
            return Host.ResolveFinalPoint(basePoint, rawPoint);
        }

        public void LogInput(Point point)
        {
            Feedback?.LogInput(string.Format(CultureInfo.InvariantCulture, "{0:0.###},{1:0.###}", point.X, point.Y));
        }

        public void LogInput(double scalar)
        {
            Feedback?.LogInput(scalar.ToString("0.###", CultureInfo.InvariantCulture));
        }

        public void LogInput(string input)
        {
            Feedback?.LogInput(input);
        }

        public void LogMessage(string message)
        {
            Feedback?.LogMessage(message);
        }

        public bool AddEntity(Entity entity, Guid? layerId = null)
        {
            if (entity == null || Document == null)
                return false;

            var targetLayerId = layerId
                ?? ActiveLayer?.Id
                ?? Document.Layers?.FirstOrDefault()?.Id;
            if (!targetLayerId.HasValue)
                return false;

            var command = new AddEntityCommand(Document, targetLayerId.Value, entity);
            if (UndoRedo != null)
                UndoRedo.Execute(command);
            else
                command.Execute();

            return true;
        }

        public Entity PickEntity(Point worldPoint, Func<Entity, bool> predicate = null)
        {
            var spatial = GetService<ISpatialQueryService>();
            var pickSettings = GetService<IPickSettingsService>();
            double pickRadius = pickSettings?.GetPickRadiusWorld(Viewport?.Zoom ?? 1.0d) ?? (4.0d / (Viewport?.Zoom ?? 1.0d));
            var candidates = spatial?.QueryNearby(worldPoint, pickRadius);
            var hits = Viewport?.QueryHitEntities(worldPoint, pickRadius, candidates) ?? Array.Empty<Entity>();
            var resolver = GetService<IPickResolutionService>();
            return resolver?.ResolvePrimary(hits, predicate) ?? hits.FirstOrDefault(entity => predicate == null || predicate(entity));
        }

        public void SetPreview(GripPreview preview)
        {
            var rubberObject = Viewport?.GetRubberObject();
            if (rubberObject == null)
                return;

            rubberObject.Preview = preview ?? GripPreview.Empty;
            rubberObject.InvalidateVisual();
        }

        public void SetEntityPreview(Entity entity, System.Windows.Media.Color color)
        {
            SetPreview(GetService<ITransientEntityPreviewService>()?.CreatePreview(entity, color) ?? GripPreview.Empty);
        }

        public void ClearPreview()
        {
            SetPreview(GripPreview.Empty);
        }

        public InteractiveCommandResult MoveToStep(CommandStep step)
        {
            return InteractiveCommandResult.MoveToStep(step);
        }

        public InteractiveCommandResult Handled()
        {
            return InteractiveCommandResult.HandledOnly();
        }

        public InteractiveCommandResult Unhandled()
        {
            return InteractiveCommandResult.Unhandled();
        }

        public InteractiveCommandResult End(string message = null, bool deactivateTool = true, bool returnToSelectionMode = true)
        {
            return InteractiveCommandResult.End(message, deactivateTool, returnToSelectionMode);
        }
    }
}
