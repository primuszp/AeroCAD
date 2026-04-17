using System.Windows;

namespace Primusz.AeroCAD.Core.Drawing
{
    public interface IViewportHostedElement
    {
        void UpdateViewportBounds(Size viewportSize);
    }
}

