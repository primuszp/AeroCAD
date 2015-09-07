using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using WpCadCore.Controls;
using WpCadCore.Model;

namespace WpCadCore.Tool
{
    class MoveTool : BaseTool, IMouseListener
    {
        public MoveTool():
            base("MoveTool")
        {

        }

        public void MouseDown(MouseEventArgs e)
        {
            if (!IsActive) return;

            ModelSpace surface = this.ToolService.ModelSpaceView as ModelSpace;
            Grip grip = VectorLayer.SelectedGrip;
            ;

        }

        public void MouseMove(MouseEventArgs e)
        {

        }

        public void MouseUp(MouseEventArgs e)
        {
            if (!IsActive) return;
            e.Handled = true;
        }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (!IsActive) return;
            e.Handled = true;
        }
    }
}
