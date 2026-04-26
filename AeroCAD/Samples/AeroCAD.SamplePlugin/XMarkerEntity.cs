using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class XMarkerEntity : Entity
    {
        public XMarkerEntity(Point center, double size)
        {
            Center = center;
            Size = size;
        }

        public Point Center { get; private set; }

        public double Size { get; private set; }

        public override int GripCount => 1;

        public override Point GetGripPoint(int index)
        {
            return Center;
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            Center = newPosition;
            InvalidateGeometry();
        }

        public override GripKind GetGripKind(int index)
        {
            return GripKind.Center;
        }

        public override Entity Clone()
        {
            var clone = new XMarkerEntity(Center, Size) { Thickness = Thickness };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate()
        {
            return new XMarkerEntity(Center, Size) { Thickness = Thickness };
        }

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as XMarkerEntity;
            if (source == null)
                return;

            Center = source.Center;
            Size = source.Size;
            RestoreBaseFrom(source);
            InvalidateGeometry();
        }

        public override void Translate(Vector delta)
        {
            Center += delta;
            InvalidateGeometry();
        }
    }
}
