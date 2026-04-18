using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Handles
{
    public class GripDescriptor
    {
        private readonly Func<Point> pointProvider;

        public GripDescriptor(Entity owner, int index, GripKind kind, Func<Point> pointProvider)
        {
            Owner = owner;
            Index = index;
            Kind = kind;
            this.pointProvider = pointProvider ?? throw new ArgumentNullException(nameof(pointProvider));
        }

        public Entity Owner { get; }

        public int Index { get; }

        public GripKind Kind { get; }

        public Point GetPoint()
        {
            return pointProvider();
        }

        public SnapType ToSnapType()
        {
            return Kind switch
            {
                GripKind.Center => SnapType.Center,
                GripKind.Midpoint => SnapType.Midpoint,
                GripKind.Quadrant => SnapType.Quadrant,
                _ => SnapType.Endpoint
            };
        }
    }
}
