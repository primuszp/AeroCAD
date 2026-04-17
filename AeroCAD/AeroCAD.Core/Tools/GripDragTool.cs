using System.Windows;
using System.Windows.Input;
using Primusz.AeroCAD.Core.Drawing.Handles;

namespace Primusz.AeroCAD.Core.Tools
{
    public class GripDragTool : InteractiveCommandToolBase, IMouseListener, IKeyboardListener
    {
        private readonly GripEditCommandController controller = new GripEditCommandController();

        public GripDragTool() : base("GripDragTool")
        { }

        public void BeginDrag(Grip grip)
        {
            controller.BeginDrag(this, grip);
            Activate();
            BeginInteractiveSession(controller.CommandName, controller.InitialStep, controller.EditorMode);
            controller.OnActivated(this);
        }

        public void MouseButtonDown(MouseEventArgs e) { }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive)
                return;

            controller.OnPointerMove(this, ToolService.Viewport.Position);
            e.Handled = true;
        }

        public void MouseButtonUp(MouseEventArgs e)
        {
            if (!IsActive || e.LeftButton != MouseButtonState.Released)
                return;

            if (ApplyResult(controller.OnLeftButtonReleased(this)))
                e.Handled = true;
        }

        public void MouseWheel(MouseWheelEventArgs e) { }

        public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ApplyResult(controller.TryCancel(this));
                e.Handled = true;
            }
        }

        public void KeyUp(KeyEventArgs e) { }

        public override bool TrySubmitToken(Primusz.AeroCAD.Core.Editor.CommandInputToken token)
        {
            return ApplyResult(controller.TrySubmitToken(this, token));
        }

        public override bool TrySubmitPoint(Point point)
        {
            return ApplyResult(controller.TrySubmitToken(this, Primusz.AeroCAD.Core.Editor.CommandInputToken.Point(FormatPoint(point), point)));
        }

        public override bool TryComplete()
        {
            return ApplyResult(controller.TryComplete(this));
        }

        public override bool TryCancel()
        {
            return ApplyResult(controller.TryCancel(this));
        }
    }
}

