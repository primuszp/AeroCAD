using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    public class PolylineTrimExtendStrategy : IEntityTrimExtendStrategy
    {
        private const double Epsilon = 1e-9;

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Polyline polyline
                && polyline.Points.Count >= 2
                && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Polyline polyline
                && polyline.Points.Count >= 2
                && boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var polyline = target as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return Array.Empty<Entity>();

            bool closed = PolylinePathOperations.IsClosed(polyline);
            if (!TryResolveClosestSegment(polyline, pickPoint, closed, out int segmentIndex, out Line segmentLine, out double clickParameter))
                return Array.Empty<Entity>();

            var intersections = GetTrimIntersections(polyline, boundaries, closed)
                .Where(item => item.Parameter > Epsilon && item.Parameter < GetPolylineLengthParameter(polyline, closed) - Epsilon)
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return Array.Empty<Entity>();

            double clickedParameter = segmentIndex + clickParameter;
            var closestIntersection = intersections
                .OrderBy(item => Math.Abs(item.Parameter - clickedParameter))
                .First();

            if (!closed && intersections.Count == 1)
            {
                // Click is on the part to remove; return the opposite side.
                if (clickedParameter >= closestIntersection.Parameter)
                {
                    // Click is after (right of) the intersection → keep the left part
                    var prefix = PolylinePathOperations.BuildOpenPath(polyline, 0d, closestIntersection.Parameter);
                    return prefix != null ? new[] { (Entity)prefix } : Array.Empty<Entity>();
                }
                else
                {
                    // Click is before (left of) the intersection → keep the right part
                    var suffix = PolylinePathOperations.BuildOpenPath(polyline, closestIntersection.Parameter, polyline.Points.Count - 1);
                    return suffix != null ? new[] { (Entity)suffix } : Array.Empty<Entity>();
                }
            }

            if (!closed)
            {
                var left = intersections.LastOrDefault(item => item.Parameter < clickedParameter - Epsilon);
                var right = intersections.FirstOrDefault(item => item.Parameter > clickedParameter + Epsilon);

                if (left == null && right != null)
                    return PolylinePathOperations.BuildOpenPath(polyline, right.Parameter, polyline.Points.Count - 1) is Polyline suffix ? new[] { (Entity)suffix } : Array.Empty<Entity>();

                if (right == null && left != null)
                    return PolylinePathOperations.BuildOpenPath(polyline, 0d, left.Parameter) is Polyline prefix ? new[] { (Entity)prefix } : Array.Empty<Entity>();

                if (left == null || right == null || ReferenceEquals(left, right))
                    return Array.Empty<Entity>();

                var results = new List<Entity>(2);
                var openPrefix = PolylinePathOperations.BuildOpenPath(polyline, 0d, left.Parameter);
                if (openPrefix != null) results.Add(openPrefix);
                var openSuffix = PolylinePathOperations.BuildOpenPath(polyline, right.Parameter, polyline.Points.Count - 1);
                if (openSuffix != null) results.Add(openSuffix);
                return results;
            }

            var closedLeft = intersections.LastOrDefault(item => item.Parameter < clickedParameter - Epsilon) ?? intersections.LastOrDefault();
            var closedRight = intersections.FirstOrDefault(item => item.Parameter > clickedParameter + Epsilon) ?? intersections.FirstOrDefault();

            if (closedLeft == null || closedRight == null || ReferenceEquals(closedLeft, closedRight))
                return Array.Empty<Entity>();

            var closedPath = PolylinePathOperations.BuildClosedPath(polyline, closedRight.Parameter, closedLeft.Parameter);
            return closedPath != null ? new[] { (Entity)closedPath } : Array.Empty<Entity>();
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var polyline = target as Polyline;
            if (polyline == null || polyline.Points.Count < 2 || PolylinePathOperations.IsClosed(polyline))
                return Array.Empty<Entity>();

            if (!TryResolveEditableSegment(polyline, pickPoint, out bool extendStart, out var segmentLine, out double clickParameter))
                return Array.Empty<Entity>();

            var intersections = GetSegmentIntersections(segmentLine, boundaries)
                .ToList();

            if (intersections.Count == 0)
                return Array.Empty<Entity>();

            Point? replacement = null;
            if (extendStart && clickParameter <= 0.5d)
            {
                replacement = intersections
                    .Where(item => item.Parameter < -Epsilon)
                    .OrderByDescending(item => item.Parameter)
                    .Select(item => (Point?)item.Point)
                    .FirstOrDefault();
            }
            else if (!extendStart && clickParameter > 0.5d)
            {
                replacement = intersections
                    .Where(item => item.Parameter > 1d + Epsilon)
                    .OrderBy(item => item.Parameter)
                    .Select(item => (Point?)item.Point)
                    .FirstOrDefault();
            }

            if (!replacement.HasValue)
                return Array.Empty<Entity>();

            var result = PolylinePathOperations.ReplaceEndpoint(polyline, extendStart, replacement.Value);
            return result != null ? new[] { (Entity)result } : Array.Empty<Entity>();
        }

        private static bool TryResolveClosestSegment(Polyline polyline, Point pickPoint, bool closed, out int segmentIndex, out Line segmentLine, out double clickParameter)
        {
            segmentIndex = PolylinePathOperations.GetClosestSegmentIndex(polyline.Points, pickPoint, closed);
            if (segmentIndex < 0)
            {
                segmentLine = null;
                clickParameter = 0d;
                return false;
            }

            var endPoint = PolylinePathOperations.GetSegmentEndPoint(polyline, segmentIndex, closed);
            segmentLine = new Line(polyline.Points[segmentIndex], endPoint) { Thickness = polyline.Thickness };
            clickParameter = ProjectParameter(segmentLine, pickPoint);
            return true;
        }

        private static bool TryResolveEditableSegment(Polyline polyline, Point pickPoint, out bool startSegment, out Line segmentLine, out double clickParameter)
        {
            startSegment = false;
            if (!TryResolveClosestSegment(polyline, pickPoint, false, out _, out segmentLine, out clickParameter))
                return false;

            startSegment = clickParameter <= 0.5d;
            return true;
        }

        private static IReadOnlyList<IntersectionPoint> GetSegmentIntersections(Line segmentLine, IReadOnlyList<Entity> boundaries)
        {
            return boundaries
                .Where(TrimExtendSupport.IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(segmentLine, boundary, restrictTargetToSegment: false))
                .Select(item => new IntersectionPoint(item.Point, item.Parameter))
                .GroupBy(item => Math.Round(item.Parameter / 1e-6))
                .Select(group => group.First())
                .OrderBy(item => item.Parameter)
                .ToList();
        }

        private static IReadOnlyList<IntersectionPoint> GetTrimIntersections(Polyline polyline, IReadOnlyList<Entity> boundaries, bool closed)
        {
            var points = closed ? PolylinePathOperations.GetRingPoints(polyline).ToList() : polyline.Points.ToList();
            var intersections = new List<IntersectionPoint>();
            int segmentCount = closed ? points.Count : points.Count - 1;

            for (int i = 0; i < segmentCount; i++)
            {
                var endPoint = closed ? points[(i + 1) % points.Count] : points[i + 1];
                var segment = new Line(points[i], endPoint) { Thickness = polyline.Thickness };

                intersections.AddRange(
                    boundaries
                        .Where(TrimExtendSupport.IsSupportedBoundary)
                        .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(segment, boundary, restrictTargetToSegment: true))
                        .Select(item => new IntersectionPoint(item.Point, i + item.Parameter)));
            }

            return intersections
                .GroupBy(item => Math.Round(item.Parameter / 1e-6))
                .Select(group => group.First())
                .OrderBy(item => item.Parameter)
                .ToList();
        }

        private static double GetPolylineLengthParameter(Polyline polyline, bool closed)
        {
            return closed ? PolylinePathOperations.GetRingPoints(polyline).Count : polyline.Points.Count - 1;
        }

        private static double ProjectParameter(Line line, Point point)
        {
            Vector direction = line.EndPoint - line.StartPoint;
            double lengthSq = direction.X * direction.X + direction.Y * direction.Y;
            if (lengthSq <= Epsilon)
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
