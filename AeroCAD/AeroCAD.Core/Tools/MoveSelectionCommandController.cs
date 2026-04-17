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
    public class MoveSelectionCommandController : CommandControllerBase
    {
        private static readonly CommandStep BasePointStep = new CommandStep("BasePoint", "Move base point:");
        private static readonly CommandStep TargetPointStep = new CommandStep("TargetPoint", "Move target point:", new[] { "ENTER" });

        private IReadOnlyList<Entity> selectedEntities = System.Array.Empty<Entity>();
        private IReadOnlyList<ModifyEntitiesCommand.EntityStateRecord> stateRecords = System.Array.Empty<ModifyEntitiesCommand.EntityStateRecord>();
        private Point basePoint;
        private bool hasBasePoint;

        public override string CommandName => "MOVE";

        public override CommandStep InitialStep => BasePointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var selectionManager = host.ToolService.GetService<ISelectionManager>();
            selectedEntities = selectionManager != null
                ? selectionManager.SelectedEntities.ToList().AsReadOnly()
                : new List<Entity>().AsReadOnly();
            stateRecords = selectedEntities
                .Select(entity => new ModifyEntitiesCommand.EntityStateRecord(entity, entity.Clone(), entity.Clone()))
                .ToList()
                .AsReadOnly();
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
            return Cancel(host, "Move command ended.");
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Cancel(host, "Move command ended.");
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

            return CommitMove(host, point - basePoint);
        }

        private InteractiveCommandResult CommitMove(IInteractiveCommandHost host, Vector displacement)
        {
            var updatedRecords = stateRecords
                .Select(record =>
                {
                    var after = record.Before.Clone();
                    after.Translate(displacement);
                    return new ModifyEntitiesCommand.EntityStateRecord(record.Target, record.Before, after);
                })
                .ToList();

            var command = new ModifyEntitiesCommand(
                updatedRecords,
                "Move Selection",
                () => host.ToolService.GetService<Drawing.Layers.Overlay>()?.Update());

            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);
            return Finish(host, "Move command ended.");
        }

        private InteractiveCommandResult Cancel(IInteractiveCommandHost host, string message)
        {
            return Finish(host, message);
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
            stateRecords = System.Array.Empty<ModifyEntitiesCommand.EntityStateRecord>();
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
