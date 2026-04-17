using System.Windows;

namespace Primusz.AeroCAD.Core.Snapping
{
    public interface ISnapDescriptor
    {
        SnapType Type { get; }

        SnapResult TrySnap(Point worldPos, double toleranceWorld);
    }
}

