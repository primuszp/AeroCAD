using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
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

        private readonly TrimExtendCommandSession session = new TrimExtendCommandSession();

        public override string CommandName => "EXTEND";

        public override CommandStep InitialStep => BoundaryStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            ClearAllBoundaryHighlights();
            session.Reset();

            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();
            if (selectionManager?.SelectedEntities.Count > 0)
            {
                var extendService = host.ToolService.GetService<IEntityTrimExtendService>();
                var supported = selectionManager.SelectedEntities.Where(entity => extendService?.CanUseAsBoundary(entity) ?? false).ToList();
                if (supported.Count > 0)
                {
                    foreach (var e in supported)
                    {
                        session.AddBoundary(e);
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

            if (!session.HasBoundaries)
            {
                ClearTargetHighlight(host);
                rubberObject.ClearPreview();
                return;
            }

            var pick = PickEntity(host, rawPoint, entity => !session.Boundaries.Contains(entity) && (extendService?.CanExtend(session.Boundaries, entity) ?? false));
            if (pick == null)
            {
                ClearTargetHighlight(host);
                rubberObject.ClearPreview();
                return;
            }

            HighlightTarget(host, pick);
            var results = extendService?.CreateExtended(session.Boundaries, pick, rawPoint);
            var previewEntity = results?.FirstOrDefault();
            var color = host.ToolService.GetService<ICadDocumentService>()?.GetLayerForEntity(pick)?.Color
                ?? System.Windows.Media.Colors.White;
            rubberObject.Preview = session.BuildPreview(host.ToolService.GetService<ITransientEntityPreviewService>(), previewEntity, color);
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            var documentService = host.ToolService.GetService<ICadDocumentService>();
            var selectionManager = host.ToolService.GetService<Selection.ISelectionManager>();
            var extendService = host.ToolService.GetService<IEntityTrimExtendService>();

            if (!IsInTargetPhase(host))
            {
                // Phase 1: pick boundary entities (multiple allowed, Enter confirms)
                var picked = PickEntity(host, rawPoint, entity => extendService?.CanUseAsBoundary(entity) ?? false);
                if (picked != null && !session.Boundaries.Contains(picked))
                {
                    if (ReferenceEquals(session.HighlightedTargetEntity, picked))
                        session.RemoveTargetHighlight();
                    session.AddBoundary(picked);
                    HighlightBoundary(picked);
                }
                return InteractiveCommandResult.HandledOnly();
            }

            // Phase 2: extend target entities
            var target = PickEntity(host, rawPoint, entity => !session.Boundaries.Contains(entity) && (extendService?.CanExtend(session.Boundaries, entity) ?? false));
            if (target == null)
                return InteractiveCommandResult.HandledOnly();

            var results = extendService?.CreateExtended(session.Boundaries, target, rawPoint);
            if (results == null || results.Count == 0)
                return InteractiveCommandResult.HandledOnly();

            // Extend always returns a single entity (closed shapes cannot be extended)
            var result = results[0];
            var overlayUpdate = (System.Action)(() => host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update());
            var command = target.GetType() == result.GetType()
                ? (IUndoableCommand)new ModifyEntityCommand(target, target.Clone(), result, "Extend Entity", overlayUpdate)
                : new ReplaceEntityCommand(documentService, target, result, "Extend Entity", overlayUpdate, selectionManager);

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            ClearTargetHighlight(host);
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

            if (session.HasBoundaries)
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
            session.Reset();
            return EndCommand(host, message);
        }

        private void HighlightBoundary(Entity entity)
        {
            if (entity == null || session.HighlightedBoundaries.Contains(entity))
                return;

            session.HighlightedBoundaries.Add(entity);
            entity.SetCommandHighlight(EntityCommandHighlightKind.Primary);
        }

        private void ClearAllBoundaryHighlights()
        {
            foreach (var entity in session.HighlightedBoundaries)
                entity.ClearCommandHighlight();
            session.HighlightedBoundaries.Clear();
        }

        private void HighlightTarget(IInteractiveCommandHost host, Entity entity)
        {
            if (ReferenceEquals(session.HighlightedTargetEntity, entity))
                return;

            ClearTargetHighlight(host);
            if (entity == null)
                return;

            session.SetTargetHighlight(entity);
            entity.SetCommandHighlight(EntityCommandHighlightKind.Hover);
        }

        private void ClearTargetHighlight(IInteractiveCommandHost host)
        {
            if (session.HighlightedTargetEntity == null)
                return;

            var entity = session.HighlightedTargetEntity;
            session.RemoveTargetHighlight();
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
