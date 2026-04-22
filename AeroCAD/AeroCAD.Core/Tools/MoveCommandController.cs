using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Tools
{
    public class MoveCommandController : CommandControllerBase
    {
        private static readonly CommandStep BasePointStep = new CommandStep("BasePoint", "Specify base point:");
        private static readonly CommandStep TargetPointStep = new CommandStep("TargetPoint", "Specify second point:");

        private readonly MoveCopyInteractiveShapeSession session = new MoveCopyInteractiveShapeSession();

        public override string CommandName => "MOVE";

        public override CommandStep InitialStep => BasePointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var selectionManager = host.ToolService.GetService<ISelectionManager>();
            session.InitializeSelection(selectionManager);

            if (selectionManager != null)
            {
                foreach (var entity in session.SelectedEntities)
                    selectionManager.Deselect(entity);
            }
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (!session.HasBasePoint)
                return;

            var finalPoint = host.ResolveFinalPoint(session.BasePoint, rawPoint);

            var rbo = host.ToolService.Viewport.GetRubberObject();
            rbo.Preview = session.BuildPreview(host.ToolService.GetService<ISelectionMovePreviewService>(), finalPoint, includeEntityPreview: true);
            rbo.InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = session.HasBasePoint
                ? host.ResolveFinalPoint(session.BasePoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final, true);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, session.HasBasePoint ? session.BasePoint : (Point?)null, out point))
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

            if (!session.HasBasePoint)
            {
                session.BeginBasePoint(point);
                return InteractiveCommandResult.MoveToStep(TargetPointStep);
            }

            return CommitMove(host, point - session.BasePoint);
        }

        private InteractiveCommandResult CommitMove(IInteractiveCommandHost host, Vector displacement)
        {
            var updatedRecords = session.StateRecords
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
            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (rbo != null)
            {
                rbo.ClearPreview();
                rbo.SnapPoint = null;
                rbo.InvalidateVisual();
            }

            return Finish(host, "Move command ended.");
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
