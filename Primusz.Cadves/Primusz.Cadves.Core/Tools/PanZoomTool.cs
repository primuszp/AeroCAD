using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Primusz.Cadves.Core.Drawing;

namespace Primusz.Cadves.Core.Tools
{
    public class PanZoomTool : BaseTool, IMouseListener
    {
        #region Members

        private Point startPoint;
        private Point startOffset;
        private double zoom = 1.0;

        #endregion

        #region Constructors

        public PanZoomTool()
            : base("PanZoomTool")
        { }

        #endregion

        #region IMouseListener Members

        public void MouseButtonUp(MouseEventArgs e)
        {
            if (!IsActive) return;

            IViewport viewport = ToolService.Viewport;

            if (viewport != null &&
                viewport.IsMouseCaptured)
            {
                viewport.Cursor = Cursors.Cross;
                viewport.ReleaseMouseCapture();
            }
        }

        public void MouseButtonDown(MouseEventArgs e)
        {
            if (!IsActive) return;

            IViewport viewport = ToolService.Viewport;

            if (viewport != null)
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    startPoint = e.GetPosition(viewport);
                    startOffset = new Point(viewport.Translate.X, viewport.Translate.Y);

                    viewport.CaptureMouse();
                    viewport.Cursor = Cursors.ScrollAll;
                }
            }
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive) return;

            IViewport viewport = ToolService.Viewport;

            if (viewport != null)
            {
                GeneralTransform inverse = viewport.ViewTransform.Inverse;

                if (inverse != null)
                {
                    viewport.Position = inverse.Transform(e.GetPosition(viewport));

                    if (viewport.IsMouseCaptured)
                    {
                        var currentPoint = e.GetPosition(viewport);
                        viewport.Translate.X = currentPoint.X - startPoint.X + startOffset.X;
                        viewport.Translate.Y = currentPoint.Y - startPoint.Y + startOffset.Y;
                    }
                }
            }
        }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (!IsActive) return;

            IViewport viewport = ToolService.Viewport;

            if (viewport != null)
            {
                zoom = viewport.Zoom;

                if (e.Delta > 0)
                {
                    zoom *= 1.2d;

                    if (zoom > 1000d)
                        zoom = 1000d;
                }
                else
                {
                    zoom /= 1.2d;

                    if (zoom < 0.001d)
                        zoom = 0.001d;
                }

                GeneralTransform inverse = viewport.ViewTransform.Inverse;

                if (inverse != null)
                {
                    var scrPoint = e.GetPosition(viewport);
                    var wrdPoint = inverse.Transform(scrPoint);

                    viewport.Translate.X = -1.0d * (wrdPoint.X * zoom - scrPoint.X);
                    viewport.Translate.Y = -1.0d * (wrdPoint.Y * zoom - scrPoint.Y);
                    viewport.Scale.ScaleX = zoom;
                    viewport.Scale.ScaleY = zoom;
                    viewport.Zoom = zoom;
                }
            }
        }

        #endregion
    }
}