using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    public class Line : Entity
    {
        #region Members

        private Point startPoint;
        private Point endPoint;

        #endregion

        #region Properties

        public Point StartPoint
        {
            get { return startPoint; }
            set { startPoint = value; InvalidateGeometry(); }
        }

        public Point EndPoint
        {
            get { return endPoint; }
            set { endPoint = value; InvalidateGeometry(); }
        }

        public override int GripCount => 3;

        #endregion

        #region Constructors

        public Line(Point start, Point end)
        {
            startPoint = start;
            endPoint = end;
        }

        #endregion

        public override Point GetGripPoint(int index)
        {
            if (index == 0) return startPoint;
            if (index == 1) return endPoint;
            return GetMidpoint();
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            if (index == 0) StartPoint = newPosition;
            else if (index == 1) EndPoint = newPosition;
            else if (index == 2)
            {
                Point midpoint = GetMidpoint();
                Vector delta = newPosition - midpoint;
                StartPoint = StartPoint + delta;
                EndPoint = EndPoint + delta;
            }
        }

        public override GripKind GetGripKind(int index)
        {
            return index == 2 ? GripKind.Midpoint : GripKind.Endpoint;
        }

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            yield return new GripDescriptor(this, 0, GripKind.Endpoint, () => StartPoint);
            yield return new GripDescriptor(this, 1, GripKind.Endpoint, () => EndPoint);
            yield return new GripDescriptor(this, 2, GripKind.Midpoint, GetMidpoint);
        }

        public override Entity Clone()
        {
            var clone = new Line(startPoint, endPoint)
            {
                Thickness = Thickness
            };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate()
        {
            return new Line(startPoint, endPoint)
            {
                Thickness = Thickness
            };
        }

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as Line;
            if (source == null) return;

            startPoint = source.StartPoint;
            endPoint = source.EndPoint;
            RestoreBaseFrom(source);
            InvalidateGeometry();
        }

        public override void Translate(Vector delta)
        {
            startPoint += delta;
            endPoint += delta;
            InvalidateGeometry();
        }

        protected override IEnumerable<ISnapDescriptor> GetAdditionalSnapDescriptors()
        {
            yield return new ComputedSnapDescriptor(SnapType.Nearest, worldPos => GetClosestPointOnLineSegment(StartPoint, EndPoint, worldPos));
        }

        private static Point GetClosestPointOnLineSegment(Point a, Point b, Point p)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            if (dx == 0 && dy == 0) return a;

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = System.Math.Max(0, System.Math.Min(1, t));

            return new Point(a.X + t * dx, a.Y + t * dy);
        }

        private Point GetMidpoint()
        {
            return new Point((StartPoint.X + EndPoint.X) / 2.0, (StartPoint.Y + EndPoint.Y) / 2.0);
        }
    }
}

