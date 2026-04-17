using System.Windows;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapResult
    {
        public SnapResult(Point point, SnapType type)
        {
            Point = point;
            Type = type;
        }

        public Point Point { get; }

        public SnapType Type { get; }
    }
}

