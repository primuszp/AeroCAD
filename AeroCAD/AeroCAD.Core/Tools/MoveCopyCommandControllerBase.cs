using System.Windows;
using Primusz.AeroCAD.Core.Commands;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Selection;

namespace Primusz.AeroCAD.Core.Tools
{
    public abstract class MoveCopyCommandControllerBase : CommandControllerBase
    {
        protected static readonly CommandStep BasePointStep = new CommandStep("BasePoint", "Specify base point:");
        protected static readonly CommandStep TargetPointStep = new CommandStep("TargetPoint", "Specify second point:");

        protected readonly MoveCopyInteractiveShapeSession session = new MoveCopyInteractiveShapeSession();

        public override CommandStep InitialStep => BasePointStep;

        public override EditorMode EditorMode => EditorMode.CommandInput;

        public override void OnActivated(IInteractiveCommandHost host)
        {
            var selectionManager = host.ToolService.GetService<ISelectionManager>();
            session.InitializeSelection(selectionManager);
            DeselectSelection(selectionManager);
            OnSelectionInitialized(host);
        }

        public override void OnPointerMove(IInteractiveCommandHost host, Point rawPoint)
        {
            UpdateSnap(host, rawPoint);

            if (!session.HasBasePoint)
                return;

            var finalPoint = host.ResolveFinalPoint(session.BasePoint, rawPoint);
            var rubberObject = host.ToolService.Viewport.GetRubberObject();
            rubberObject.Preview = session.BuildPreview(
                host.ToolService.GetService<ISelectionMovePreviewService>(),
                finalPoint,
                ShouldIncludeEntityPreview());
            rubberObject.InvalidateVisual();
        }

        public override InteractiveCommandResult TrySubmitViewportPoint(IInteractiveCommandHost host, Point rawPoint)
        {
            Point final = session.HasBasePoint
                ? host.ResolveFinalPoint(session.BasePoint, rawPoint)
                : host.ResolveFinalPoint(null, rawPoint);

            return SubmitResolvedPoint(host, final);
        }

        public override InteractiveCommandResult TrySubmitToken(IInteractiveCommandHost host, CommandInputToken token)
        {
            Point point;
            if (!host.TryResolvePointInput(token, session.HasBasePoint ? session.BasePoint : (Point?)null, out point))
                return InteractiveCommandResult.Unhandled();

            return SubmitResolvedPoint(host, point);
        }

        public override InteractiveCommandResult TryComplete(IInteractiveCommandHost host)
        {
            return Finish(host, EndedMessage);
        }

        public override InteractiveCommandResult TryCancel(IInteractiveCommandHost host)
        {
            return Finish(host, EndedMessage);
        }

        protected abstract string EndedMessage { get; }

        protected virtual bool ShouldIncludeEntityPreview() => true;

        protected virtual void OnSelectionInitialized(IInteractiveCommandHost host)
        {
        }

        protected abstract InteractiveCommandResult CommitDisplacement(IInteractiveCommandHost host, Vector displacement);

        private InteractiveCommandResult SubmitResolvedPoint(IInteractiveCommandHost host, Point point)
        {
            host.ToolService.GetService<ICommandFeedbackService>()?.LogInput(InteractiveCommandToolBase.FormatPoint(point));

            if (!session.HasBasePoint)
            {
                session.BeginBasePoint(point);
                return InteractiveCommandResult.MoveToStep(TargetPointStep);
            }

            return CommitDisplacement(host, point - session.BasePoint);
        }

        protected InteractiveCommandResult CommitWithOverlayUpdate(IInteractiveCommandHost host, IUndoableCommand command, bool clearPreview = true)
        {
            host.ToolService.GetService<IUndoRedoService>()?.Execute(command);

            if (clearPreview)
                ClearRubberPreview(host);

            return InteractiveCommandResult.HandledOnly();
        }

        private void DeselectSelection(ISelectionManager selectionManager)
        {
            if (selectionManager == null)
                return;

            foreach (var entity in session.SelectedEntities)
                selectionManager.Deselect(entity);
        }

        private InteractiveCommandResult Finish(IInteractiveCommandHost host, string message)
        {
            session.Reset();
            ClearRubberPreview(host);
            return EndCommand(host, message, returnToSelectionMode: ShouldReturnToSelectionModeOnFinish());
        }

        protected virtual bool ShouldReturnToSelectionModeOnFinish() => false;
    }
}
