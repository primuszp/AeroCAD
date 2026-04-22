using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editing.Offsets;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.Core.Tools
{
    public class OffsetCommandController : CommandControllerBase
    {
        private static readonly CommandStep OffsetInputStep = new CommandStep("OffsetInput", "Specify offset distance:");
        private static readonly CommandStep EntityStep = new CommandStep("Entity", "Select object to offset [Exit/Undo]:", inputMode: CommandInputMode.Selection);
        private static readonly CommandStep SidePointStep = new CommandStep("SidePoint", "Specify point on side to offset:");

        private readonly OffsetInteractiveShapeSession session = new OffsetInteractiveShapeSession();
        private System.Windows.Media.Color sourceColor;

        public override string CommandName => "OFFSET";

        public override CommandStep InitialStep => null;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();
            var document = host.ToolService.GetService<ICadDocumentService>();
            session.Reset();

            var sourceEntity = selectionManager?.SelectedEntities.Count == 1 ? selectionManager.SelectedEntities[0] : null;
            if (sourceEntity != null)
            {
                var layer = document?.GetLayerForEntity(sourceEntity);
                session.BeginSelection(sourceEntity, layer?.Id ?? System.Guid.Empty);
                selectionManager?.Deselect(sourceEntity);
            }

            UpdateSourceColor(document, sourceEntity);
            host.MoveToStep(OffsetInputStep);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (!session.FixedDistance.HasValue)
            {
                host.ToolService.Viewport.GetRubberObject()?.ClearPreview();
                return;
            }

            if (session.SourceEntity == null)
            {
                HighlightPickCandidate(host, rawPoint);
                return;
            }

            var previewPoint = host.ResolveFinalPoint(null, rawPoint);
            var previewEntity = host.ToolService.GetService<IEntityOffsetService>()?.CreateOffsetByDistance(
                session.SourceEntity,
                session.FixedDistance.Value,
                previewPoint);

            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            var previewService = host.ToolService.GetService<ITransientEntityPreviewService>();
            rubberObject.Preview = session.BuildPreview(previewService, previewEntity, sourceColor);
            rubberObject.InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            if (!session.FixedDistance.HasValue)
                return InteractiveCommandResult.HandledOnly();

            if (host.CurrentStep?.Id == EntityStep.Id || session.SourceEntity == null)
            {
                var picked = PickEntity(host, rawPoint);
                if (picked == null)
                    return InteractiveCommandResult.HandledOnly();

                var document = host.ToolService.GetService<ICadDocumentService>();
                var layer = document?.GetLayerForEntity(picked);
                session.BeginSelection(picked, layer?.Id ?? System.Guid.Empty);
                UpdateSourceColor(document, picked);
                return InteractiveCommandResult.MoveToStep(SidePointStep);
            }

            return SubmitResolvedPoint(host, host.ResolveFinalPoint(null, rawPoint), true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (!session.FixedDistance.HasValue && host.CurrentStep?.Id == OffsetInputStep.Id)
            {
                double scalar;
                if (host.TryResolveScalarInput(token, out scalar))
                {
                    session.SetFixedDistance(scalar);
                    return session.SourceEntity != null
                        ? InteractiveCommandResult.MoveToStep(SidePointStep)
                        : InteractiveCommandResult.MoveToStep(EntityStep);
                }
            }

            if (token?.TextValue?.Trim().ToUpperInvariant() == "EXIT")
                return Finish(host, "Offset command ended.");

            Point point;
            if (!host.TryResolvePointInput(token, null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "Offset command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "Offset command ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            if (!session.IsReady || !session.FixedDistance.HasValue)
                return Finish(host, "Offset command ended.");

            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            var offsetService = host.ToolService.GetService<IEntityOffsetService>();
            var resultEntity = offsetService?.CreateOffsetByDistance(session.SourceEntity, session.FixedDistance.Value, point);

            if (resultEntity == null)
                return InteractiveCommandResult.HandledOnly();

            var document = host.ToolService.GetService<ICadDocumentService>();
            var command = new AddEntityCommand(document, session.SourceLayerId, resultEntity);
            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);

            session.ResetSelection();
            return InteractiveCommandResult.MoveToStep(EntityStep);
        }

        private void HighlightPickCandidate(IInteractiveCommandHost host, Point rawPoint)
        {
            // Clear rubber preview while in entity-selection phase
            host.ToolService.Viewport.GetRubberObject()?.ClearPreview();
        }

        private static Entity PickEntity(IInteractiveCommandHost host, Point point)
        {
            var spatial = host.ToolService.GetService<ISpatialQueryService>();
            var pickSettings = host.ToolService.GetService<IPickSettingsService>();
            double pickRadius = pickSettings?.GetPickRadiusWorld(host.ToolService.Viewport.Zoom) ?? (4.0d / host.ToolService.Viewport.Zoom);
            var candidates = spatial?.QueryNearby(point, pickRadius) ?? System.Array.Empty<Entity>();
            var hits = host.ToolService.Viewport.QueryHitEntities(point, pickRadius, candidates);
            var pickResolver = host.ToolService.GetService<Selection.IPickResolutionService>();
            return pickResolver?.ResolvePrimary(hits, null) ?? System.Linq.Enumerable.FirstOrDefault(hits);
        }

        private void UpdateSourceColor(ICadDocumentService document, Entity sourceEntity)
        {
            var sourceLayer = document?.GetLayerForEntity(sourceEntity);
            sourceColor = sourceLayer?.Color ?? System.Windows.Media.Colors.White;
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            session.Reset();
            return EndCommand(host, message);
        }
    }
}
