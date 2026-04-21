using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.GeometryMath;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    public class Arc : Entity
    {
        private const double Epsilon = 1e-9;

        private Point center;
        private double radius;
        private double startAngle;
        private double sweepAngle;

        public Arc(Point center, double radius, double startAngle, double sweepAngle)
        {
            this.center = center;
            this.radius = Math.Abs(radius);
            this.startAngle = CircularGeometry.NormalizeAngle(startAngle);
            this.sweepAngle = CircularGeometry.NormalizeSweep(sweepAngle);
        }

        public Point Center
        {
            get => center;
            set
            {
                center = value;
                InvalidateGeometry();
            }
        }

        public double Radius
        {
            get => radius;
            set
            {
                radius = Math.Abs(value);
                InvalidateGeometry();
            }
        }

        public double StartAngle
        {
            get => startAngle;
            set
            {
                startAngle = CircularGeometry.NormalizeAngle(value);
                InvalidateGeometry();
            }
        }

        public double SweepAngle
        {
            get => sweepAngle;
            set
            {
                sweepAngle = CircularGeometry.NormalizeSweep(value);
                InvalidateGeometry();
            }
        }

        public double EndAngle => CircularGeometry.NormalizeAngle(startAngle + sweepAngle);

        public Point StartPoint => CircularGeometry.GetPoint(center, radius, startAngle);

        public Point EndPoint => CircularGeometry.GetPoint(center, radius, EndAngle);

        public Point MidPoint => CircularGeometry.GetPoint(center, radius, startAngle + (sweepAngle / 2d));

        public override int GripCount => 4;

        public override Point GetGripPoint(int index)
        {
            switch (index)
            {
                case 0: return StartPoint;
                case 1: return EndPoint;
                case 2: return MidPoint;
                case 3: return center;
                default: return center;
            }
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            int direction = sweepAngle >= 0d ? 1 : -1;

            switch (index)
            {
                case 0:
                {
                    double endAngle = EndAngle;
                    double newStartAngle = CircularGeometry.GetAngle(center, newPosition);
                    double newSweep = CircularGeometry.GetDirectionalDistance(newStartAngle, endAngle, direction);
                    if (direction < 0)
                        newSweep = -newSweep;

                    if (Math.Abs(newSweep) > 0.001d)
                    {
                        startAngle = CircularGeometry.NormalizeAngle(newStartAngle);
                        sweepAngle = CircularGeometry.NormalizeSweep(newSweep);
                        InvalidateGeometry();
                    }

                    break;
                }
                case 1:
                {
                    double newEndAngle = CircularGeometry.GetAngle(center, newPosition);
                    double newSweep = CircularGeometry.GetDirectionalDistance(startAngle, newEndAngle, direction);
                    if (direction < 0)
                        newSweep = -newSweep;

                    if (Math.Abs(newSweep) > 0.001d)
                    {
                        sweepAngle = CircularGeometry.NormalizeSweep(newSweep);
                        InvalidateGeometry();
                    }

                    break;
                }
                case 2:
                {
                    double newRadius = (newPosition - center).Length;
                    if (newRadius > 0.001d)
                    {
                        radius = newRadius;
                        InvalidateGeometry();
                    }

                    break;
                }
                case 3:
                {
                    center = newPosition;
                    InvalidateGeometry();
                    break;
                }
            }
        }

        public override GripKind GetGripKind(int index)
        {
            if (index == 2)
                return GripKind.Midpoint;

            if (index == 3)
                return GripKind.Center;

            return GripKind.Endpoint;
        }

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            yield return new GripDescriptor(this, 0, GripKind.Endpoint, () => StartPoint);
            yield return new GripDescriptor(this, 1, GripKind.Endpoint, () => EndPoint);
            yield return new GripDescriptor(this, 2, GripKind.Midpoint, () => MidPoint);
            yield return new GripDescriptor(this, 3, GripKind.Center, () => center);
        }

        public override Entity Clone()
        {
            var clone = new Arc(center, radius, startAngle, sweepAngle)
            {
                Thickness = Thickness
            };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate()
        {
            return new Arc(center, radius, startAngle, sweepAngle)
            {
                Thickness = Thickness
            };
        }

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as Arc;
            if (source == null)
                return;

            center = source.Center;
            radius = source.Radius;
            startAngle = source.StartAngle;
            sweepAngle = source.SweepAngle;
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
            yield return new ComputedSnapDescriptor(SnapType.Nearest, GetClosestPointOnArc);
        }

        public static Geometry BuildGeometry(Point center, double radius, double startAngle, double sweepAngle)
        {
            if (radius <= Epsilon || Math.Abs(sweepAngle) <= Epsilon)
                return Geometry.Empty;

            var startPoint = CircularGeometry.GetPoint(center, radius, startAngle);
            var endPoint = CircularGeometry.GetPoint(center, radius, startAngle + sweepAngle);
            var isLargeArc = Math.Abs(sweepAngle) > Math.PI;
            // The viewport applies a Y-axis flip (world Y up -> screen Y down).
            // ArcSegment sweep direction is evaluated before that transform, so the visual
            // direction must be inverted here to keep the rendered arc consistent with the
            // world-space math used by grips, snapping and hit testing.
            var sweepDirection = sweepAngle >= 0d ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;

            var figure = new PathFigure
            {
                StartPoint = startPoint,
                IsClosed = false,
                IsFilled = false
            };

            figure.Segments.Add(new ArcSegment(
                endPoint,
                new Size(radius, radius),
                0d,
                isLargeArc,
                sweepDirection,
                true));

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        private Point GetClosestPointOnArc(Point worldPos)
        {
            if (radius <= Epsilon)
                return center;

            double angle = CircularGeometry.GetAngle(center, worldPos);
            if (CircularGeometry.IsAngleOnArc(angle, startAngle, sweepAngle))
                return CircularGeometry.GetPoint(center, radius, angle);

            return (StartPoint - worldPos).LengthSquared <= (EndPoint - worldPos).LengthSquared
                ? StartPoint
                : EndPoint;
        }
    }
}
