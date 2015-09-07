using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WpCadCore.Controls;
using System.Windows.Input;

namespace WpCadCore.Tool
{
    class PanZoomTool : BaseTool, IMouseListener
    {
        private Point startOffset;
        private Point startPoint;
        private double zoom = 1d;

        public PanZoomTool()
            : base("PanZoomTool")
        { }

        #region IMouseListener Members

        public void MouseUp(MouseEventArgs e)
        {
            if (!IsActive) return;

            ModelSpace canvas = this.ToolService.ModelSpaceView as ModelSpace;

            if (canvas.IsMouseCaptured)
            {
                canvas.Cursor = Cursors.Cross;
                canvas.ReleaseMouseCapture();
            }
        }

        public void MouseDown(MouseEventArgs e)
        {
            if (!IsActive) return;

            ModelSpace canvas = this.ToolService.ModelSpaceView as ModelSpace;

            if (e.RightButton == MouseButtonState.Pressed)
            {
                this.startPoint = e.GetPosition(canvas);
                this.startOffset = new Point(canvas.Translate.X, canvas.Translate.Y);

                canvas.CaptureMouse();
                canvas.Cursor = Cursors.ScrollAll;
            }
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive) return;

            ModelSpace canvas = this.ToolService.ModelSpaceView as ModelSpace;

            if (canvas.IsMouseCaptured)
            {
                var currentPoint = e.GetPosition(canvas);
                canvas.Translate.X = currentPoint.X - this.startPoint.X + this.startOffset.X;
                canvas.Translate.Y = currentPoint.Y - this.startPoint.Y + this.startOffset.Y;
            }
        }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (!IsActive) return;

            ModelSpace canvas = this.ToolService.ModelSpaceView as ModelSpace;

            this.zoom = canvas.Zoom;

            if (e.Delta > 0)
            {
                this.zoom *= 1.2d;

                if (this.zoom > 1000d)
                    this.zoom = 1000d;
            }
            else
            {
                this.zoom /= 1.2d;

                if (this.zoom < 0.001d)
                    this.zoom = 0.001d;
            }

            var scrPoint = e.GetPosition(canvas);
            var wrdPoint = canvas.ViewTransform.Inverse.Transform(scrPoint);

            canvas.Translate.X = -1 * (wrdPoint.X * this.zoom - scrPoint.X);
            canvas.Translate.Y = -1 * (wrdPoint.Y * this.zoom - scrPoint.Y);
            canvas.Scale.ScaleX = this.zoom;
            canvas.Scale.ScaleY = this.zoom;
            canvas.Zoom = this.zoom;
        }

        #endregion
    }
}
