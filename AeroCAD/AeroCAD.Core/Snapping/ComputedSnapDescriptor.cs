using System;
using System.Windows;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class ComputedSnapDescriptor : ISnapDescriptor
    {
        private readonly Func<Point, Point> pointEvaluator;

        public ComputedSnapDescriptor(SnapType type, Func<Point, Point> pointEvaluator)
        {
            Type = type;
            this.pointEvaluator = pointEvaluator ?? throw new ArgumentNullException(nameof(pointEvaluator));
        }

        public SnapType Type { get; }

        public SnapResult TrySnap(Point worldPos, double toleranceWorld)
        {
            Point point = pointEvaluator(worldPos);
            double dx = point.X - worldPos.X;
            double dy = point.Y - worldPos.Y;

            return Math.Sqrt(dx * dx + dy * dy) <= toleranceWorld
                ? new SnapResult(point, Type)
                : null;
        }
    }
}

