using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Snapping
{
    public class SnapPointDescriptor : ISnapDescriptor
    {
        private readonly Func<Point> pointProvider;

        public SnapPointDescriptor(SnapType type, Func<Point> pointProvider, Entity sourceEntity = null, int? sourceGripIndex = null)
        {
            Type = type;
            this.pointProvider = pointProvider ?? throw new ArgumentNullException(nameof(pointProvider));
            SourceEntity = sourceEntity;
            SourceGripIndex = sourceGripIndex;
        }

        public SnapType Type { get; }

        public Entity SourceEntity { get; }

        public int? SourceGripIndex { get; }

        public SnapResult TrySnap(Point worldPos, double toleranceWorld)
        {
            Point point = pointProvider();
            double dx = point.X - worldPos.X;
            double dy = point.Y - worldPos.Y;

            return Math.Sqrt(dx * dx + dy * dy) <= toleranceWorld
                ? new SnapResult(point, Type, point, SourceEntity, SourceGripIndex)
                : null;
        }
    }
}
