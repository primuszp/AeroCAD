using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Documents;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Tools
{
    public class GripEditCommandController : CommandControllerBase
    {
        private static readonly CommandStep GripPointStep = new CommandStep("GripPoint", "Specify stretch point:");

        private readonly GripEditInteractiveShapeSession session = new GripEditInteractiveShapeSession();

        public override string CommandName => "GRIP";

        public override CommandStep InitialStep => GripPointStep;

        public override EditorMode EditorMode => EditorMode.GripEditing;

        public void BeginDrag(IInteractiveCommandHost host, Grip grip)
        {
            session.BeginDrag(grip);
        }

        public override void OnActivated(IInteractiveCommandHost host)
        {
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            if (!session.HasGrip)
                return;

            var rbo = host.ToolService.Viewport.GetRubberObject();
            UpdateSnap(host, rawPoint);

            session.UpdatePreview(host.ResolveFinalPoint(session.DragBasePoint, rawPoint));
            var gripPreviewService = host.ToolService.GetService<IGripPreviewService>();
            rbo.Preview = session.BuildPreview(gripPreviewService);
            rbo.InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            return InteractiveCommandResult.Unhandled();
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            if (!session.HasGrip)
                return InteractiveCommandResult.Unhandled();

            Point point;
            if (!host.TryResolvePointInput(token, session.DragBasePoint, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point);
        }

        public override InteractiveCommandResult OnLeftButtonReleased(IInteractiveCommandHost host)
        {
            if (!session.HasGrip)
                return InteractiveCommandResult.Unhandled();

            if (session.IgnoreNextMouseUp)
            {
                session.ConsumeInitialMouseUp();
                return InteractiveCommandResult.HandledOnly();
            }

            return Commit(host);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            if (!session.HasGrip)
                return InteractiveCommandResult.Unhandled();

            return Commit(host);
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            if (!session.HasGrip)
                return InteractiveCommandResult.Unhandled();

            return Finish(host, "Grip edit ended.");
        }

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point)
        {
            var snapEngine = host.ToolService.GetService<Snapping.ISnapEngine>();
            session.UpdatePreview(snapEngine?.Snap(point) ?? point);
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(session.PreviewPosition));

            var rbo = host.ToolService.Viewport.GetRubberObject();
            var gripPreviewService = host.ToolService.GetService<IGripPreviewService>();
            rbo.Preview = session.BuildPreview(gripPreviewService);
            rbo.InvalidateVisual();
            return InteractiveCommandResult.HandledOnly();
        }

        private InteractiveCommandResult Commit(IInteractiveCommandHost host)
        {
            var stateAfterDrag = session.StateBeforeDrag.Clone();
            stateAfterDrag.MoveGrip(session.ActiveGrip.Index, session.PreviewPosition);

            var cmd = new ModifyEntityCommand(
                session.ActiveGrip.Owner,
                session.StateBeforeDrag,
                stateAfterDrag,
                "Move Grip",
                () => host.ToolService.GetService<Overlay>()?.Update());

            host.ToolService.GetService<IUndoRedoService>()?.Execute(cmd);
            return Finish(host, "Grip edit ended.");
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            ClearRubberPreview(host);
            session.ActiveGrip?.Unselect();
            session.Reset();
            return EndCommand(host, message, returnToSelectionMode: false);
        }
    }
}
