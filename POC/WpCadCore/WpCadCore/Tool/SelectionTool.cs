using System.Windows;
using System.Windows.Input;
using WpCadCore.Controls;

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
            if (!IsActive) return;

            ModelSpace surface = this.ToolService.ModelSpaceView as ModelSpace;
            Point wpos = surface.WorldPosition;

            if (this.rbl == null)
                this.rbl = surface.RubberLine;

            this.rbl.CurrentStyle = RubberLine.RubberStyle.Select;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                //Kijelölés mutatóval
                bool selected = surface.HitTest(wpos);

                if (this.rbl.CurrentState == RubberLine.RubberState.Rubber)
                {
                    this.rbl.SetStop(wpos);
                    this.end = this.rbl.End;

                    //Kijelölés dobozzal
                    surface.HitTest(new Rect(start, end));
                }
                else if (selected == false)
                {
                    this.rbl.SetStart(wpos);
                    this.rbl.SetMove(wpos);

                    this.start = this.rbl.Start;
                    this.end = this.start;
                }
            }

            e.Handled = true;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive) return;

            ModelSpace canvas = this.ToolService.ModelSpaceView as ModelSpace;

            if (this.rbl == null)
                this.rbl = canvas.RubberLine;

            this.rbl.SetMove(canvas.WorldPosition);
            e.Handled = true;
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

        #endregion
    }
}
