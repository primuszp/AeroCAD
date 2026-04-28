using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class PointEntity : Entity
    {
        public PointEntity(Point location)
        {
            Location = location;
        }

        public Point Location { get; private set; }

        public override int GripCount => 1;

        public override Point GetGripPoint(int index)
        {
            return Location;
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            Location = newPosition;
            InvalidateGeometry();
        }

        public override GripKind GetGripKind(int index)
        {
            return GripKind.Node;
        }

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            yield return new GripDescriptor(this, 0, GripKind.Node, () => Location);
        }

        protected override IEnumerable<ISnapDescriptor> GetAdditionalSnapDescriptors()
        {
            yield return new SnapPointDescriptor(SnapType.Node, () => Location, this, 0);
        }

        public override Entity Clone()
        {
            var clone = new PointEntity(Location) { Thickness = Thickness };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate()
        {
            return new PointEntity(Location) { Thickness = Thickness };
        }

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as PointEntity;
            if (source == null)
                return;

            Location = source.Location;
            RestoreBaseFrom(source);
            InvalidateGeometry();
        }

        public override void Translate(Vector delta)
        {
            Location += delta;
            InvalidateGeometry();
        }
    }
}
