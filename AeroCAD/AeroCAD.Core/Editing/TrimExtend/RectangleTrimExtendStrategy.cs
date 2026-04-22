using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class RectangleTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Rectangle && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return false;
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var rect = target as Rectangle;
            if (rect == null)
                return Array.Empty<Entity>();

            var polyline = ToClosedPolyline(rect);
            var ringPoints = PolylinePathOperations.GetRingPoints(polyline);
            if (ringPoints.Count < 3)
                return Array.Empty<Entity>();

            int segmentIndex = PolylinePathOperations.GetClosestSegmentIndex(ringPoints, pickPoint, closed: true);
            if (segmentIndex < 0)
                return Array.Empty<Entity>();

            var segmentStart = ringPoints[segmentIndex];
            var segmentEnd = PolylinePathOperations.GetSegmentEndPoint(polyline, segmentIndex, closed: true);
            var closestPoint = GetClosestPointOnSegment(segmentStart, segmentEnd, pickPoint);
            var clickParameter = ProjectParameter(new Line(segmentStart, segmentEnd), closestPoint);

            var intersections = GetPerimeterIntersections(polyline, boundaries);
            if (intersections.Count < 2)
                return Array.Empty<Entity>();

            double clickedParameter = segmentIndex + clickParameter;
            var left = intersections.LastOrDefault(item => item.Parameter < clickedParameter)
                ?? intersections.LastOrDefault();
            var right = intersections.FirstOrDefault(item => item.Parameter > clickedParameter)
                ?? intersections.FirstOrDefault();
            if (left == null || right == null || ReferenceEquals(left, right))
                return Array.Empty<Entity>();

            var path = PolylinePathOperations.BuildClosedPath(polyline, right.Parameter, left.Parameter);
            return path != null ? new[] { (Entity)path } : Array.Empty<Entity>();
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            return Array.Empty<Entity>();
        }

        private static Polyline ToClosedPolyline(Rectangle rect)
        {
            var tl = rect.TopLeft;
            var br = rect.BottomRight;
            var tr = new Point(br.X, tl.Y);
            var bl = new Point(tl.X, br.Y);

            return new Polyline(new[] { tl, tr, br, bl, tl })
            {
                Thickness = rect.Thickness
            };
        }
        private static IReadOnlyList<IntersectionPoint> GetPerimeterIntersections(Polyline polyline, IReadOnlyList<Entity> boundaries)
        {
            var points = PolylinePathOperations.GetRingPoints(polyline);
            var intersections = new List<IntersectionPoint>();
            for (int i = 0; i < points.Count; i++)
            {
                var segment = new Line(points[i], points[(i + 1) % points.Count]) { Thickness = polyline.Thickness };
                intersections.AddRange(
                    TrimExtendSupport.GetSupportedBoundaries(boundaries)
                        .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(segment, boundary))
                        .Select(item => new IntersectionPoint(item.Point, i + item.Parameter)));
            }

            return intersections
                .GroupBy(item => Math.Round(item.Parameter / 1e-6))
                .Select(g => g.First())
                .OrderBy(item => item.Parameter)
                .ToList();
        }

        private static Point GetClosestPointOnSegment(Point start, Point end, Point point)
        {
            Vector direction = end - start;
            double lengthSq = direction.LengthSquared;
            if (lengthSq <= 1e-9)
                return start;

            double t = Vector.Multiply(point - start, direction) / lengthSq;
            t = Math.Max(0d, Math.Min(1d, t));
            return start + (direction * t);
        }

        private static double ProjectParameter(Line line, Point point)
        {
            Vector direction = line.EndPoint - line.StartPoint;
            double lengthSq = direction.X * direction.X + direction.Y * direction.Y;
            if (lengthSq <= 1e-9)
                return 0d;

            Vector toPoint = point - line.StartPoint;
            return ((toPoint.X * direction.X) + (toPoint.Y * direction.Y)) / lengthSq;
        }

        private sealed class IntersectionPoint
        {
            public IntersectionPoint(Point point, double parameter)
            {
                Point = point;
                Parameter = parameter;
            }

            public Point Point { get; }

            public double Parameter { get; }
        }
    }
}
