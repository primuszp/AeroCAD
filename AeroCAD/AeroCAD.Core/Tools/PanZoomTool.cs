using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Tools
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

        public override int InputPriority => 200;

        #endregion

        #region IMouseListener Members

        public void MouseButtonUp(MouseEventArgs e)
        {
            if (!IsActive) return;

            IViewport viewport = ToolService.Viewport;

            if (viewport != null &&
                viewport.IsMouseCaptured)
            {
                viewport.Cursor = Cursors.None;
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
                var currentPoint = e.GetPosition(viewport);
                viewport.Position = viewport.Unproject(currentPoint);

                if (viewport.IsMouseCaptured)
                {
                    viewport.Translate.X = currentPoint.X - startPoint.X + startOffset.X;
                    viewport.Translate.Y = currentPoint.Y - startPoint.Y + startOffset.Y;
                    viewport.RefreshView();
                }

                ToolService.GetService<Overlay>().Update();
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

                var element = viewport as FrameworkElement;
                if (element != null)
                {
                    var scrPoint = e.GetPosition(viewport);
                    var wrdPoint = viewport.Unproject(scrPoint);

                    viewport.Zoom = zoom;
                    viewport.Translate.X = -1.0d * (wrdPoint.X * zoom - scrPoint.X);
                    viewport.Translate.Y = scrPoint.Y + (wrdPoint.Y * zoom) - element.ActualHeight;
                    viewport.RefreshView();
                    viewport.Position = viewport.Unproject(scrPoint);
                }

                // Update snap tolerance to maintain constant 10 screen-pixel snap distance
                var snapEngine = ToolService.GetService<ISnapEngine>();
                if (snapEngine != null)
                    snapEngine.ToleranceWorld = 10.0 / zoom;

                ToolService.GetService<Overlay>().Update();
            }
        }

        #endregion
    }
}

