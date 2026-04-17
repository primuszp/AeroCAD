using System;
using System.Windows;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapPointDescriptor : ISnapDescriptor
    {
        private readonly Func<Point> pointProvider;

        public SnapPointDescriptor(SnapType type, Func<Point> pointProvider)
        {
            Type = type;
            this.pointProvider = pointProvider ?? throw new ArgumentNullException(nameof(pointProvider));
        }

        public SnapType Type { get; }

        public SnapResult TrySnap(Point worldPos, double toleranceWorld)
        {
            Point point = pointProvider();
            double dx = point.X - worldPos.X;
            double dy = point.Y - worldPos.Y;

            return Math.Sqrt(dx * dx + dy * dy) <= toleranceWorld
                ? new SnapResult(point, Type)
                : null;
        }
    }
}

