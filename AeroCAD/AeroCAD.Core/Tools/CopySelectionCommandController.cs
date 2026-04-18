using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Tools
{
    public class CopySelectionCommandController : CommandControllerBase
    {
        private static readonly CommandStep BasePointStep = new CommandStep("BasePoint", "Specify base point:");
        private static readonly CommandStep TargetPointStep = new CommandStep("TargetPoint", "Specify second point:");

        private IReadOnlyList<Entity> selectedEntities = System.Array.Empty<Entity>();
        private readonly Dictionary<System.Guid, System.Guid> sourceLayers = new Dictionary<System.Guid, System.Guid>();
        private Point basePoint;
        private bool hasBasePoint;

        public override string CommandName => "COPY";

        public override CommandStep InitialStep => BasePointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var selectionManager = host.ToolService.GetService<ISelectionManager>();
            var document = host.ToolService.GetService<ICadDocumentService>();

            selectedEntities = selectionManager != null
                ? selectionManager.SelectedEntities.ToList().AsReadOnly()
                : new List<Entity>().AsReadOnly();

            sourceLayers.Clear();
            if (document != null)
            {
                foreach (var entity in selectedEntities)
                {
                    var layer = document.GetLayerForEntity(entity);
                    if (layer != null)
                        sourceLayers[entity.Id] = layer.Id;
                }
            }

            hasBasePoint = false;
            basePoint = default(Point);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (!hasBasePoint)
                return;

            var finalPoint = host.ResolveFinalPoint(basePoint, rawPoint);
            var displacement = finalPoint - basePoint;

            var rbo = host.ToolService.Viewport.GetRubberObject();
            rbo.CurrentStyle = Drawing.Layers.RubberStyle.Line;
            rbo.SetStart(basePoint);
            rbo.SetMove(finalPoint);
            rbo.Preview = BuildPreview(host, displacement, finalPoint);
            rbo.InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = hasBasePoint
                ? host.ResolveFinalPoint(basePoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, hasBasePoint ? basePoint : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point, true);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, "Copy command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, "Copy command ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point, bool logInput)
        {
            if (logInput)
                host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!hasBasePoint)
            {
                hasBasePoint = true;
                basePoint = point;
                var rbo = host.ToolService.Viewport.GetRubberObject();
                rbo.CurrentStyle = Drawing.Layers.RubberStyle.Line;
                rbo.SetStart(basePoint);
                return InteractiveCommandResult.MoveToStep(TargetPointStep);
            }

            CommitCopy(host, point - basePoint);
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            rubberObject.SetStart(basePoint);
            rubberObject.SetMove(point);
            return InteractiveCommandResult.MoveToStep(TargetPointStep);
        }

        private void CommitCopy(IInteractiveCommandHost host, Vector displacement)
        {
            var document = host.ToolService.GetService<ICadDocumentService>();
            if (document == null)
                return;

            var records = selectedEntities
                .Where(entity => entity != null && sourceLayers.ContainsKey(entity.Id))
                .Select(entity =>
                {
                    var duplicate = entity.Duplicate();
                    duplicate.Translate(displacement);
                    return new AddEntitiesCommand.AddedEntityRecord(sourceLayers[entity.Id], duplicate);
                })
                .ToList();

            if (records.Count == 0)
                return;

            var command = new AddEntitiesCommand(
                document,
                records,
                records.Count == 1 ? "Copy Entity" : "Copy Entities");

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update();
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (rbo != null)
            {
                rbo.SnapPoint = null;
                rbo.ClearPreview();
                rbo.Cancel();
                rbo.InvalidateVisual();
            }

            selectedEntities = System.Array.Empty<Entity>();
            sourceLayers.Clear();
            hasBasePoint = false;
            basePoint = default(Point);

            return InteractiveCommandResult.End(message, deactivateTool: true, returnToSelectionMode: true);
        }

        private GripPreview BuildPreview(IInteractiveCommandHost host, Vector displacement, Point currentPoint)
        {
            var movePreviewService = host.ToolService.GetService<ISelectionMovePreviewService>();
            var preview = movePreviewService?.CreatePreview(selectedEntities, displacement) ?? GripPreview.Empty;
            var strokes = preview.Strokes.ToList();
            strokes.Add(GripPreviewStroke.CreateScreenConstant(new LineGeometry(basePoint, currentPoint), Colors.Orange, 1.5d, DashStyles.Dash));
            return new GripPreview(strokes);
        }
    }
}
