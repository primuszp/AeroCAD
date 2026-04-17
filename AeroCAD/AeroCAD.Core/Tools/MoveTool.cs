using System.Windows;
using System.Windows.Input;

namespace Primusz.AeroCAD.Core.Tools
{
    public class MoveTool : InteractiveCommandToolBase, IMouseListener, IKeyboardListener
    {
        private readonly MoveSelectionCommandController controller = new MoveSelectionCommandController();

        public MoveTool() : base("MoveTool")
        { }

        public void MouseButtonDown(MouseEventArgs e)
        {
            if (!IsActive || IsSuspended || e.LeftButton != MouseButtonState.Pressed)
                return;

            ApplyResult(controller.TrySubmitViewportPoint(this, ToolService.Viewport.Position));
            e.Handled = true;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive || IsSuspended)
                return;

            controller.OnPointerMove(this, ToolService.Viewport.Position);
            e.Handled = true;
        }

        public void MouseButtonUp(MouseEventArgs e) { }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (!IsActive || IsSuspended)
                return;

            ToolService.Viewport.GetRubberObject().InvalidateVisual();
            e.Handled = true;
        }

        public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ApplyResult(controller.TryCancel(this));
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                ApplyResult(controller.TryComplete(this));
                e.Handled = true;
            }
        }

        public void KeyUp(KeyEventArgs e) { }

        public override bool Activate()
        {
            var activated = base.Activate();
            if (activated)
            {
                BeginInteractiveSession(controller.CommandName, controller.InitialStep, controller.EditorMode);
                controller.OnActivated(this);
            }

            return activated;
        }

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

