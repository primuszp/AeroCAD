using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class GripEditCommandController : CommandControllerBase
    {
        private static readonly CommandStep GripPointStep = new CommandStep("GripPoint", "Specify stretch point:");

        private Grip activeGrip;
        private Entity stateBeforeDrag;
        private bool ignoreNextMouseUp;
        private Point previewPosition;
        private Point dragBasePoint;

        public override string CommandName => "GRIP";

        public override CommandStep InitialStep => GripPointStep;

        public override EditorMode EditorMode => EditorMode.GripEditing;

        public void BeginDrag(IInteractiveCommandHost host, Grip grip)
        {
            activeGrip = grip;
            stateBeforeDrag = grip.Owner.Clone();
            ignoreNextMouseUp = true;
            previewPosition = grip.Owner.GetGripPoint(grip.Index);
            dragBasePoint = previewPosition;
            activeGrip.Select();
        }

        public override void OnActivated(IInteractiveCommandHost host)
        {
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            if (activeGrip == null)
                return;

            var rbo = host.ToolService.Viewport.GetRubberObject();
            UpdateSnap(host, rawPoint);

            previewPosition = host.ResolveFinalPoint(dragBasePoint, rawPoint);
            var gripPreviewService = host.ToolService.GetService<IGripPreviewService>();
            rbo.Preview = gripPreviewService?.CreatePreview(stateBeforeDrag, activeGrip.Index, previewPosition);
            rbo.InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            return InteractiveCommandResult.Unhandled();
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (activeGrip == null)
                return InteractiveCommandResult.Unhandled();

            Point point;
            if (!host.TryResolvePointInput(token, dragBasePoint, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point);
        }

        public override InteractiveCommandResult OnLeftButtonReleased(IInteractiveCommandHost host)
        {
            if (activeGrip == null)
                return InteractiveCommandResult.Unhandled();

            if (ignoreNextMouseUp)
            {
                ignoreNextMouseUp = false;
                return InteractiveCommandResult.HandledOnly();
            }

            return Commit(host);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            if (activeGrip == null)
                return InteractiveCommandResult.Unhandled();

            return Commit(host);
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            if (activeGrip == null)
                return InteractiveCommandResult.Unhandled();

            return Finish(host, "Grip edit ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point)
        {
            var snapEngine = host.ToolService.GetService<Snapping.ISnapEngine>();
            previewPosition = snapEngine?.Snap(point) ?? point;
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(previewPosition));

            var rbo = host.ToolService.Viewport.GetRubberObject();
            var gripPreviewService = host.ToolService.GetService<IGripPreviewService>();
            rbo.Preview = gripPreviewService?.CreatePreview(stateBeforeDrag, activeGrip.Index, previewPosition);
            rbo.InvalidateVisual();
            return InteractiveCommandResult.HandledOnly();
        }

        private InteractiveCommandResult Commit(IInteractiveCommandHost host)
        {
            var stateAfterDrag = stateBeforeDrag.Clone();
            stateAfterDrag.MoveGrip(activeGrip.Index, previewPosition);

            var cmd = new ModifyEntityCommand(
                activeGrip.Owner,
                stateBeforeDrag,
                stateAfterDrag,
                "Move Grip",
                () => host.ToolService.GetService<Overlay>()?.Update());

            host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
            return Finish(host, "Grip edit ended.");
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            var rbo = host.ToolService.Viewport.GetRubberObject();
            if (rbo != null)
            {
                rbo.SnapPoint = null;
                rbo.ClearPreview();
                rbo.InvalidateVisual();
            }

            activeGrip?.Unselect();
            activeGrip = null;
            stateBeforeDrag = null;
            ignoreNextMouseUp = false;
            dragBasePoint = default(Point);
            return InteractiveCommandResult.End(message, deactivateTool: true);
        }
    }
}
