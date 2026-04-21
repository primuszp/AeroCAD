using System.Windows;
using System.Windows.Controls;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public abstract class ViewportHostedScreenLayerBase : FrameworkElement, IViewportSpaceElement, IViewportHostedElement
    {
        protected readonly Viewport viewport;

        public ViewportCoordinateSpace CoordinateSpace => ViewportCoordinateSpace.Screen;

        protected ViewportHostedScreenLayerBase(Viewport viewport, int zIndex)
        {
            this.viewport = viewport;
            IsHitTestVisible = false;

            viewport.Children.Add(this);
            Panel.SetZIndex(this, zIndex);
        }

        public virtual void UpdateViewportBounds(Size viewportSize)
        {
            Width = viewportSize.Width;
            Height = viewportSize.Height;
        }
    }
}
