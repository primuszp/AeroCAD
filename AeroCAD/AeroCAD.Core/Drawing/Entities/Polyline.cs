using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Entities
{
    public class Polyline : Entity
    {
        #region Members

        private readonly List<Point> points = new List<Point>();

        #endregion

        #region Properties

        public IReadOnlyList<Point> Points => points.AsReadOnly();

        public override int GripCount => points.Count;

        #endregion

        #region Constructors

        public Polyline(IEnumerable<Point> initialPoints)
        {
            if (initialPoints != null)
                points.AddRange(initialPoints);
        }

        #endregion

        #region Methods

        public override Point GetGripPoint(int index)
        {
            return points[index];
        }

        public override void MoveGrip(int index, Point newPosition)
        {
            SetPoint(index, newPosition);
        }

        public override GripKind GetGripKind(int index)
        {
            return GripKind.Endpoint;
        }

        public override IEnumerable<GripDescriptor> GetGripDescriptors()
        {
            for (int i = 0; i < points.Count; i++)
            {
                int currentIndex = i;
                yield return new GripDescriptor(this, currentIndex, GripKind.Endpoint, () => points[currentIndex]);
            }
        }

        public override Entity Clone()
        {
            var clone = new Polyline(points)
            {
                Thickness = Thickness
            };
            CopyIdentityTo(clone);
            return clone;
        }

        public override Entity Duplicate()
        {
            return new Polyline(points)
            {
                Thickness = Thickness
            };
        }

        public override void RestoreState(Entity sourceState)
        {
            var source = sourceState as Polyline;
            if (source == null) return;

            points.Clear();
            points.AddRange(source.points);
            Thickness = source.Thickness;
            InvalidateGeometry();
        }

        public override void Translate(Vector delta)
        {
            for (int i = 0; i < points.Count; i++)
                points[i] = points[i] + delta;

            InvalidateGeometry();
        }

        protected override IEnumerable<ISnapDescriptor> GetAdditionalSnapDescriptors()
        {
            if (points.Count == 0)
                yield break;

            for (int i = 0; i < points.Count - 1; i++)
            {
                int currentIndex = i;
                int nextIndex = i + 1;
                yield return new SnapPointDescriptor(
                    SnapType.Midpoint,
                    () => new Point((points[currentIndex].X + points[nextIndex].X) / 2.0, (points[currentIndex].Y + points[nextIndex].Y) / 2.0));
            }

            yield return new ComputedSnapDescriptor(SnapType.Nearest, GetClosestPoint);
        }

        public void SetPoint(int index, Point value)
        {
            points[index] = value;
            InvalidateGeometry();
        }

        public void AddPoint(Point p)
        {
            points.Add(p);
            InvalidateGeometry();
        }

        public void RemoveLastPoint()
        {
            if (points.Count == 0)
                return;

            points.RemoveAt(points.Count - 1);
            InvalidateGeometry();
        }

        internal static Geometry BuildGeometry(IReadOnlyList<Point> sourcePoints)
        {
            var geo = new PathGeometry();
            if (sourcePoints == null || sourcePoints.Count < 2)
                return geo;

            var figure = new PathFigure { StartPoint = sourcePoints[0], IsClosed = false };

            for (int i = 1; i < sourcePoints.Count; i++)
                figure.Segments.Add(new LineSegment(sourcePoints[i], true));

            geo.Figures.Add(figure);
            return geo;
        }

        private Point GetClosestPoint(Point worldPos)
        {
            if (points.Count < 2)
                return points.Count == 1 ? points[0] : worldPos;

            Point bestPoint = points[0];
            double minDistanceSq = double.MaxValue;

            for (int i = 0; i < points.Count - 1; i++)
            {
                Point a = points[i];
                Point b = points[i + 1];

                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double t = 0;

                if (dx != 0 || dy != 0)
                {
                    t = ((worldPos.X - a.X) * dx + (worldPos.Y - a.Y) * dy) / (dx * dx + dy * dy);
                    t = System.Math.Max(0, System.Math.Min(1, t));
                }

                Point closest = new Point(a.X + t * dx, a.Y + t * dy);
                double distSq = (closest.X - worldPos.X) * (closest.X - worldPos.X) + (closest.Y - worldPos.Y) * (closest.Y - worldPos.Y);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    bestPoint = closest;
                }
            }

            return bestPoint;
        }

        #endregion
    }
}

