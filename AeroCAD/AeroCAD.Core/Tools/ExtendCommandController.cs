using System.Collections.Generic;
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
    public class ExtendCommandController : CommandControllerBase
    {
        private static readonly CommandStep BoundaryStep = new CommandStep("Boundary", "Select boundary edges [Enter=confirm]:", inputMode: CommandInputMode.Selection);
        private static readonly CommandStep TargetStep = new CommandStep("Target", "Select object to extend [Enter=end]:", inputMode: CommandInputMode.Selection);

        private readonly List<Entity> boundaryEntities = new List<Entity>();
        private readonly List<Entity> highlightedBoundaries = new List<Entity>();
        private Entity highlightedTargetEntity;

        public override string CommandName => "EXTEND";

        public override CommandStep InitialStep => BoundaryStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            ClearAllBoundaryHighlights();
            boundaryEntities.Clear();

            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();
            if (selectionManager?.SelectedEntities.Count > 0)
            {
                var supported = selectionManager.SelectedEntities.Where(IsSupportedBoundary).ToList();
                if (supported.Count > 0)
                {
                    foreach (var e in supported)
                    {
                        boundaryEntities.Add(e);
                        HighlightBoundary(e);
                    }
                    host.MoveToStep(TargetStep);
                    return;
                }
            }
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            var extendService = host.ToolService.GetService<IEntityTrimExtendService>();

            if (boundaryEntities.Count == 0)
            {
                ClearTargetHighlight(host);
                rubberObject.ClearPreview();
                return;
            }

            var pick = PickEntity(host, rawPoint, entity => !boundaryEntities.Contains(entity) && (extendService?.CanExtend(boundaryEntities, entity) ?? false));
            if (pick == null)
            {
                ClearTargetHighlight(host);
                rubberObject.ClearPreview();
                return;
            }

            HighlightTarget(host, pick);
            var previewEntity = extendService?.CreateExtended(boundaryEntities, pick, rawPoint);
            var color = host.ToolService.GetService<ICadDocumentService>()?.GetLayerForEntity(pick)?.Color
                ?? System.Windows.Media.Colors.White;
            rubberObject.Preview = host.ToolService.GetService<ITransientEntityPreviewService>()?.CreatePreview(previewEntity, color);
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            var documentService = host.ToolService.GetService<ICadDocumentService>();
            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();

            if (!IsInTargetPhase(host))
            {
                // Phase 1: pick boundary entities (multiple allowed, Enter confirms)
                var picked = PickEntity(host, rawPoint, IsSupportedBoundary);
                if (picked != null && !boundaryEntities.Contains(picked))
                {
                    // Detach from target-highlight tracking before promoting to boundary
                    if (ReferenceEquals(highlightedTargetEntity, picked))
                    {
                        highlightedTargetEntity = null;
                    }
                    boundaryEntities.Add(picked);
                    HighlightBoundary(picked);
                }
                return InteractiveCommandResult.HandledOnly();
            }

            // Phase 2: extend target entities
            var extendService = host.ToolService.GetService<IEntityTrimExtendService>();
            var target = PickEntity(host, rawPoint, entity => !boundaryEntities.Contains(entity) && (extendService?.CanExtend(boundaryEntities, entity) ?? false));
            if (target == null)
                return InteractiveCommandResult.HandledOnly();

            var result = extendService?.CreateExtended(boundaryEntities, target, rawPoint);
            if (result == null)
                return InteractiveCommandResult.HandledOnly();

            var command = target.GetType() == result.GetType()
                ? (IUndoableCommand)new ModifyEntityCommand(
                    target,
                    target.Clone(),
                    result,
                    "Extend Entity",
                    () => host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update())
                : new ReplaceEntityCommand(
                    documentService,
                    target,
                    result,
                    "Extend Entity",
                    () => host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update(),
                    selectionManager);

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            ClearTargetHighlight(host);
            // Stay in target step — user can extend more
            return InteractiveCommandResult.HandledOnly();
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
            if (IsInTargetPhase(host))
                return Finish(host, "Extend command ended.");

            // Enter in boundary phase: if none selected, use all entities as boundary edges
            if (boundaryEntities.Count == 0)
            {
                var document = host.ToolService.GetService<ICadDocumentService>();
                if (document != null)
                {
                    foreach (var e in document.Entities)
                    {
                        if (IsSupportedBoundary(e) && !boundaryEntities.Contains(e))
                        {
                            boundaryEntities.Add(e);
                            HighlightBoundary(e);
                        }
                    }
                }
            }

            if (boundaryEntities.Count > 0)
            {
                host.MoveToStep(TargetStep);
                return InteractiveCommandResult.HandledOnly();
            }

            return Finish(host, "Extend command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "Extend command ended.");
        }

        private bool IsInTargetPhase(IInteractiveCommandHost host)
        {
            return host.CurrentStep?.Id == TargetStep.Id;
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            ClearAllBoundaryHighlights();
            ClearTargetHighlight(host);
            boundaryEntities.Clear();
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            rubberObject.ClearPreview();
            rubberObject.SnapPoint = null;
            rubberObject.Cancel();
            rubberObject.InvalidateVisual();
            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }

        private static bool IsSupportedBoundary(Entity entity)
        {
            return entity is Line || entity is Polyline || entity is Circle || entity is Arc || entity is Rectangle;
        }

        private void HighlightBoundary(Entity entity)
        {
            if (entity == null || highlightedBoundaries.Contains(entity))
                return;

            highlightedBoundaries.Add(entity);
            entity.SetCommandHighlight(EntityCommandHighlightKind.Primary);
        }

        private void ClearAllBoundaryHighlights()
        {
            foreach (var entity in highlightedBoundaries)
                entity.ClearCommandHighlight();
            highlightedBoundaries.Clear();
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
            var pickSettings = host.ToolService.GetService<IPickSettingsService>();
            double pickRadius = pickSettings?.GetPickRadiusWorld(host.ToolService.Viewport.Zoom) ?? (4.0d / host.ToolService.Viewport.Zoom);
            var candidates = spatial?.QueryNearby(point, pickRadius) ?? System.Array.Empty<Entity>();
            var hits = host.ToolService.Viewport.QueryHitEntities(point, pickRadius, candidates);
            var pickResolver = host.ToolService.GetService<Selection.IPickResolutionService>();
            return pickResolver?.ResolvePrimary(hits, predicate) ?? hits.FirstOrDefault(entity => predicate == null || predicate(entity));
        }
    }
}
