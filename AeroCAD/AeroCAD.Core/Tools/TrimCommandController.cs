using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.Core.Tools
{
    public class TrimCommandController : CommandControllerBase
    {
        private static readonly CommandStep BoundaryStep = new CommandStep("Boundary", "Select cutting edge:", inputMode: CommandInputMode.Selection);
        private static readonly CommandStep TargetStep = new CommandStep("Target", "Select object to trim:", inputMode: CommandInputMode.Selection);

        private Entity boundaryEntity;
        private Entity highlightedBoundaryEntity;
        private Entity highlightedTargetEntity;

        public override string CommandName => "TRIM";

        public override CommandStep InitialStep => BoundaryStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            ClearBoundaryHighlight(host);
            boundaryEntity = null;

            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();
            if (selectionManager?.SelectedEntities.Count == 1 && IsSupportedBoundary(selectionManager.SelectedEntities[0]))
            {
                boundaryEntity = selectionManager.SelectedEntities[0];
                HighlightBoundary(host, boundaryEntity);
                host.MoveToStep(TargetStep);
            }
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            var trimService = host.ToolService.GetService<IEntityTrimExtendService>();
            if (boundaryEntity == null)
            {
                ClearTargetHighlight(host);
                rubberObject.ClearPreview();
                return;
            }

            var pick = PickEntity(host, rawPoint, entity => entity != boundaryEntity && (trimService?.CanTrim(boundaryEntity, entity) ?? false));
            if (pick == null)
            {
                ClearTargetHighlight(host);
                rubberObject.ClearPreview();
                return;
            }

            HighlightTarget(host, pick);
            var previewEntity = trimService?.CreateTrimmed(boundaryEntity, pick, rawPoint);
            var color = host.ToolService.GetService<ICadDocumentService>()?.GetLayerForEntity(pick)?.Color
                ?? System.Windows.Media.Colors.White;
            rubberObject.Preview = host.ToolService.GetService<ITransientEntityPreviewService>()?.CreatePreview(previewEntity, color);
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            var documentService = host.ToolService.GetService<ICadDocumentService>();
            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();

            if (boundaryEntity == null)
            {
                boundaryEntity = PickEntity(host, rawPoint, IsSupportedBoundary);
                HighlightBoundary(host, boundaryEntity);
                return boundaryEntity != null
                    ? InteractiveCommandResult.MoveToStep(TargetStep)
                    : InteractiveCommandResult.HandledOnly();
            }

            var trimService = host.ToolService.GetService<IEntityTrimExtendService>();
            var target = PickEntity(host, rawPoint, entity => entity != boundaryEntity && (trimService?.CanTrim(boundaryEntity, entity) ?? false));
            if (target == null)
                return InteractiveCommandResult.HandledOnly();

            var result = trimService?.CreateTrimmed(boundaryEntity, target, rawPoint);
            if (result == null)
                return InteractiveCommandResult.HandledOnly();

            var command = target.GetType() == result.GetType()
                ? (IUndoableCommand)new ModifyEntityCommand(
                    target,
                    target.Clone(),
                    result,
                    "Trim Entity",
                    () => host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update())
                : new ReplaceEntityCommand(
                    documentService,
                    target,
                    result,
                    "Trim Entity",
                    () => host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update(),
                    selectionManager);

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            return Finish(host, "Trim command ended.");
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (token?.PointValue == null)
                return InteractiveCommandResult.Unhandled();

            return TrySubmitViewportPoint(host, token.PointValue.Value);
        }

        public override InteractiveCommandResult OnLeftButtonReleased(IInteractiveCommandHost host)
        {
            return InteractiveCommandResult.Unhandled();
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "Trim command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "Trim command ended.");
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            ClearBoundaryHighlight(host);
            ClearTargetHighlight(host);
            boundaryEntity = null;
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            rubberObject.ClearPreview();
            rubberObject.SnapPoint = null;
            rubberObject.Cancel();
            rubberObject.InvalidateVisual();
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }

        private static bool IsSupportedBoundary(Entity entity)
        {
            return entity is Line || entity is Polyline || entity is Circle || entity is Arc;
        }

        private void HighlightBoundary(IInteractiveCommandHost host, Entity entity)
        {
            if (ReferenceEquals(highlightedBoundaryEntity, entity))
                return;

            ClearBoundaryHighlight(host);
            if (entity == null)
                return;

            highlightedBoundaryEntity = entity;
            entity.SetCommandHighlight(EntityCommandHighlightKind.Primary);
        }

        private void ClearBoundaryHighlight(IInteractiveCommandHost host)
        {
            if (highlightedBoundaryEntity == null)
                return;

            var entity = highlightedBoundaryEntity;
            highlightedBoundaryEntity = null;
            entity.ClearCommandHighlight();
        }

        private void HighlightTarget(IInteractiveCommandHost host, Entity entity)
        {
            if (ReferenceEquals(highlightedTargetEntity, entity))
                return;

            ClearTargetHighlight(host);
            if (entity == null)
                return;

            highlightedTargetEntity = entity;
            entity.SetCommandHighlight(EntityCommandHighlightKind.Hover);
        }

        private void ClearTargetHighlight(IInteractiveCommandHost host)
        {
            if (highlightedTargetEntity == null)
                return;

            var entity = highlightedTargetEntity;
            highlightedTargetEntity = null;
            entity.ClearCommandHighlight();
        }

        private static Entity PickEntity(IInteractiveCommandHost host, Point point, System.Func<Entity, bool> predicate)
        {
            var spatial = host.ToolService.GetService<ISpatialQueryService>();
            double pickRadius = 4.0d / host.ToolService.Viewport.Zoom;
            var candidates = spatial?.QueryNearby(point, pickRadius) ?? System.Array.Empty<Entity>();
            var hits = host.ToolService.Viewport.QueryHitEntities(point, candidates);
            var pickResolver = host.ToolService.GetService<Selection.IPickResolutionService>();
            return pickResolver?.ResolvePrimary(hits, predicate) ?? hits.FirstOrDefault(entity => predicate == null || predicate(entity));
        }
    }
}
