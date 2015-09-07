using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using WpCadCore.Controls;
using System.Windows;
using WpCadCore.Model;

namespace WpCadCore.Tool
{
    class SelectionTool : BaseTool, IMouseListener
    {
        private RubberLine rbl;
        private Point start;
        private Point end;

        public SelectionTool()
            : base("SelectionTool")
        { }

        #region IMouseListener Members

        public void MouseDown(MouseEventArgs e)
        {
            ModelSpace canvas = this.ToolService.ModelSpaceView as ModelSpace;
            Point pos = canvas.WorldPosition;

            if (this.rbl == null) 
                this.rbl = canvas.RubberLine;

            this.rbl.CurrentStyle = RubberLine.RubberStyle.Select;

            if ((canvas as ISelectionService).HitTest(pos))
            {
                ;
            }

            if (start != end)
            {
                if ((canvas as ISelectionService).HitTest(new Rect(start, end)))
                {
                    ;
                }
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.rbl.CurrentState == RubberLine.RubberState.Rubber)
                {
                    this.rbl.SetStop(pos);
                    end = this.rbl.End;
                }
                else
                {
                    this.rbl.SetStart(pos);
                    this.rbl.SetMove(pos);

                    start = this.rbl.Start;
                    end = start;
                }
            }

            //e.Handled = true;
        }

        public void MouseMove(MouseEventArgs e)
        {
            ModelSpace canvas = this.ToolService.ModelSpaceView as ModelSpace;
            if (this.rbl == null) this.rbl = canvas.RubberLine;
            this.rbl.SetMove(canvas.WorldPosition);
        }

        public void MouseUp(System.Windows.Input.MouseEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void MouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
