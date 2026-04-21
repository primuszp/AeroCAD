using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    public class Rectangle : Entity
    {
        private Point topLeft;
        private Point bottomRight;

        public Point TopLeft
        {
            get => topLeft;
            set { topLeft = value; InvalidateGeometry(); }
        }

        public Point BottomRight
        {
            get => bottomRight;
            set { bottomRight = value; InvalidateGeometry(); }
        }

        public double Width => Math.Abs(bottomRight.X - topLeft.X);
        public double Height => Math.Abs(bottomRight.Y - topLeft.Y);

        // Grips: 4 corners (Endpoint) + 1 center (Center for translate)
        public override int GripCount => 5;

        public Rectangle(Point corner1, Point corner2)
        {
            topLeft = new Point(Math.Min(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y));
            bottomRight = new Point(Math.Max(corner1.X, corner2.X), Math.Max(corner1.Y, corner2.Y));
        }

        public override Point GetGripPoint(int index)
        {
            switch (index)
            {
                case 0: return topLeft;
                case 1: return new Point(bottomRight.X, topLeft.Y);
                case 2: return bottomRight;
                case 3: return new Point(topLeft.X, bottomRight.Y);
                case 4: return GetCenter();
                default: return topLeft;
            }
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            switch (index)
            {
                case 0: // TopLeft → update left edge and top edge
                    TopLeft = new Point(newPosition.X, newPosition.Y);
                    break;
                case 1: // TopRight → update right edge and top edge
                    topLeft = new Point(topLeft.X, newPosition.Y);
                    bottomRight = new Point(newPosition.X, bottomRight.Y);
                    InvalidateGeometry();
                    break;
                case 2: // BottomRight → update right edge and bottom edge
                    BottomRight = new Point(newPosition.X, newPosition.Y);
                    break;
                case 3: // BottomLeft → update left edge and bottom edge
                    topLeft = new Point(newPosition.X, topLeft.Y);
                    bottomRight = new Point(bottomRight.X, newPosition.Y);
                    InvalidateGeometry();
                    break;
                case 4: // Center → translate entire rectangle
                    Point center = GetCenter();
                    Vector delta = newPosition - center;
                    topLeft += delta;
                    bottomRight += delta;
                    InvalidateGeometry();
                    break;
            }
        }

        public override GripKind GetGripKind(int index)
        {
            return index == 4 ? GripKind.Center : GripKind.Endpoint;
        }

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            yield return new GripDescriptor(this, 0, GripKind.Endpoint, () => topLeft);
            yield return new GripDescriptor(this, 1, GripKind.Endpoint, () => new Point(bottomRight.X, topLeft.Y));
            yield return new GripDescriptor(this, 2, GripKind.Endpoint, () => bottomRight);
            yield return new GripDescriptor(this, 3, GripKind.Endpoint, () => new Point(topLeft.X, bottomRight.Y));
            yield return new GripDescriptor(this, 4, GripKind.Center, GetCenter);
        }

        protected override IEnumerable<ISnapDescriptor> GetAdditionalSnapDescriptors()
        {
            // Edge midpoints as snap points
            yield return new SnapPointDescriptor(SnapType.Midpoint, () => new Point((topLeft.X + bottomRight.X) / 2, topLeft.Y));
            yield return new SnapPointDescriptor(SnapType.Midpoint, () => new Point(bottomRight.X, (topLeft.Y + bottomRight.Y) / 2));
            yield return new SnapPointDescriptor(SnapType.Midpoint, () => new Point((topLeft.X + bottomRight.X) / 2, bottomRight.Y));
            yield return new SnapPointDescriptor(SnapType.Midpoint, () => new Point(topLeft.X, (topLeft.Y + bottomRight.Y) / 2));

            // Nearest point on any edge
            yield return new ComputedSnapDescriptor(SnapType.Nearest, worldPos => GetNearestPointOnPerimeter(worldPos));
        }

        public override Entity Clone()
        {
            var clone = new Rectangle(topLeft, bottomRight) { Thickness = Thickness };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate()
        {
            return new Rectangle(topLeft, bottomRight) { Thickness = Thickness };
        }

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as Rectangle;
            if (source == null) return;

            topLeft = source.topLeft;
            bottomRight = source.bottomRight;
            RestoreBaseFrom(source);
            InvalidateGeometry();
        }

        public override void Translate(Vector delta)
        {
            topLeft += delta;
            bottomRight += delta;
            InvalidateGeometry();
        }

        private Point GetCenter()
        {
            return new Point((topLeft.X + bottomRight.X) / 2, (topLeft.Y + bottomRight.Y) / 2);
        }

        private Point GetNearestPointOnPerimeter(Point p)
        {
            // Find closest point on any of the 4 edges
            var corners = new[]
            {
                topLeft,
                new Point(bottomRight.X, topLeft.Y),
                bottomRight,
                new Point(topLeft.X, bottomRight.Y)
            };

            Point best = topLeft;
            double bestDist = double.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                Point a = corners[i];
                Point b = corners[(i + 1) % 4];
                Point nearest = GetClosestPointOnSegment(a, b, p);
                double dist = (nearest - p).LengthSquared;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = nearest;
                }
            }

            return best;
        }

        private static Point GetClosestPointOnSegment(Point a, Point b, Point p)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            if (dx == 0 && dy == 0) return a;

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            return new Point(a.X + t * dx, a.Y + t * dy);
        }
    }
}
