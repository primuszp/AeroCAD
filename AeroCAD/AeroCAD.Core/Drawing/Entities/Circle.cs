using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    public class Circle : Entity
    {
        private Point center;
        private double radius;

        public Circle(Point center, double radius)
        {
            this.center = center;
            this.radius = Math.Abs(radius);
        }

        public Point Center
        {
            get { return center; }
            set
            {
                center = value;
                InvalidateGeometry();
            }
        }

        public double Radius
        {
            get { return radius; }
            set
            {
                radius = Math.Abs(value);
                InvalidateGeometry();
            }
        }

        public override int GripCount => 5;

        public override Point GetGripPoint(int index)
        {
            switch (index)
            {
                case 0:
                    return center;
                case 1:
                    return new Point(center.X + radius, center.Y);
                case 2:
                    return new Point(center.X, center.Y - radius);
                case 3:
                    return new Point(center.X - radius, center.Y);
                case 4:
                    return new Point(center.X, center.Y + radius);
                default:
                    return center;
            }
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            if (index == 0)
            {
                Center = newPosition;
                return;
            }

            Radius = (newPosition - center).Length;
        }

        public override GripKind GetGripKind(int index)
        {
            return index == 0 ? GripKind.Center : GripKind.Quadrant;
        }

        public override Entity Clone()
        {
            var clone = new Circle(center, radius)
            {
                Thickness = Thickness
            };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate()
        {
            return new Circle(center, radius)
            {
                Thickness = Thickness
            };
        }

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as Circle;
            if (source == null)
                return;

            center = source.Center;
            radius = source.Radius;
            RestoreBaseFrom(source);
            InvalidateGeometry();
        }

        public override void Translate(Vector delta)
        {
            center += delta;
            InvalidateGeometry();
        }

        protected override IEnumerable<ISnapDescriptor> GetAdditionalSnapDescriptors()
        {
            yield return new ComputedSnapDescriptor(SnapType.Nearest, GetClosestPointOnCircle);
        }

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            yield return new GripDescriptor(this, 0, GripKind.Center, () => center);
            yield return new GripDescriptor(this, 1, GripKind.Quadrant, () => new Point(center.X + radius, center.Y));
            yield return new GripDescriptor(this, 2, GripKind.Quadrant, () => new Point(center.X, center.Y - radius));
            yield return new GripDescriptor(this, 3, GripKind.Quadrant, () => new Point(center.X - radius, center.Y));
            yield return new GripDescriptor(this, 4, GripKind.Quadrant, () => new Point(center.X, center.Y + radius));
        }

        public static Geometry BuildGeometry(Point center, double radius)
        {
            return radius > 0d
                ? new EllipseGeometry(center, radius, radius)
                : Geometry.Empty;
        }

        private Point GetClosestPointOnCircle(Point worldPos)
        {
            if (radius <= 0d)
                return center;

            Vector direction = worldPos - center;
            if (direction.LengthSquared <= double.Epsilon)
                return new Point(center.X + radius, center.Y);

            direction.Normalize();
            return center + (direction * radius);
        }
    }
}
