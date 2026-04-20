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
        private const double ParameterDedupTolerance = 1e-6;

        public bool CanTrim(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Polyline polyline &&
                polyline.Points.Count >= 2 &&
                boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Polyline polyline &&
                polyline.Points.Count >= 2 &&
                boundaries.Any(TrimExtendSupport.IsSupportedBoundary);
        }

        public IReadOnlyList<Entity> CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var polyline = target as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return Array.Empty<Entity>();

            bool isClosed = IsClosed(polyline);

            if (!TryResolveClosestSegment(polyline, pickPoint, isClosed, out int segmentIndex, out var segmentLine, out double clickParameter))
                return Array.Empty<Entity>();

            var intersections = GetPolylineIntersections(polyline, boundaries)
                .Where(item => item.Parameter > Epsilon && item.Parameter < GetTotalParameter(polyline, isClosed) - Epsilon)
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return Array.Empty<Entity>();

            double clickedParameter = segmentIndex + clickParameter;
            if (isClosed && clickedParameter < 0d)
                clickedParameter += GetTotalParameter(polyline, isClosed);

            var left = GetPreviousIntersection(intersections, clickedParameter, isClosed, GetTotalParameter(polyline, isClosed));
            var right = GetNextIntersection(intersections, clickedParameter, isClosed, GetTotalParameter(polyline, isClosed));

            if (left == null && right == null)
                return Array.Empty<Entity>();

            if (!isClosed)
            {
                double total = GetTotalParameter(polyline, false);

                // Only a "right" boundary: click is before it → remove [start, right], keep [right, end]
                if (left == null)
                {
                    var suffix = BuildPath(polyline, right.Parameter, total, includeStart: false, includeEnd: true);
                    return suffix != null ? new[] { (Entity)suffix } : Array.Empty<Entity>();
                }

                // Only a "left" boundary: click is after it → remove [left, end], keep [start, left]
                if (right == null)
                {
                    var prefix = BuildPath(polyline, 0d, left.Parameter, includeStart: true, includeEnd: false);
                    return prefix != null ? new[] { (Entity)prefix } : Array.Empty<Entity>();
                }
            }

            if (left == null || right == null || ReferenceEquals(left, right))
                return Array.Empty<Entity>();

            return isClosed
                ? BuildClosedTrimResult(polyline, left, right)
                : BuildOpenTrimResult(polyline, left, right);
        }

        public IReadOnlyList<Entity> CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var polyline = target as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return Array.Empty<Entity>();

            if (IsClosed(polyline))
                return Array.Empty<Entity>();

            if (!TryResolveEditableSegment(polyline, pickPoint, out bool extendStart, out var segmentLine, out double clickParameter))
                return Array.Empty<Entity>();

            var intersections = boundaries
                .Where(TrimExtendSupport.IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(segmentLine, boundary, restrictTargetToSegment: false))
                .ToList();

            if (intersections.Count < 1)
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
            else if (!extendStart && clickParameter >= 0.5d)
            {
                replacement = intersections
                    .Where(item => item.Parameter > 1d + Epsilon)
                    .OrderBy(item => item.Parameter)
                    .Select(item => (Point?)item.Point)
                    .FirstOrDefault();
            }

            if (!replacement.HasValue)
                return Array.Empty<Entity>();

            var result = CreateEndpointPolyline(polyline, extendStart, replacement.Value);
            return result != null ? new[] { (Entity)result } : Array.Empty<Entity>();
        }

        private static IReadOnlyList<Entity> BuildOpenTrimResult(Polyline source, IntersectionPoint left, IntersectionPoint right)
        {
            var results = new List<Entity>(2);

            var prefix = BuildPath(source, 0, left.Parameter, includeStart: true, includeEnd: false);
            if (prefix != null) results.Add(prefix);

            var suffix = BuildPath(source, right.Parameter, GetTotalParameter(source, false), includeStart: false, includeEnd: true);
            if (suffix != null) results.Add(suffix);

            return results;
        }

        private static IReadOnlyList<Entity> BuildClosedTrimResult(Polyline source, IntersectionPoint left, IntersectionPoint right)
        {
            var path = BuildClosedPath(source, right.Parameter, left.Parameter);
            return path != null ? new[] { (Entity)path } : Array.Empty<Entity>();
        }

        private static bool TryResolveClosestSegment(Polyline polyline, Point pickPoint, bool isClosed, out int segmentIndex, out Line segmentLine, out double clickParameter)
        {
            segmentIndex = -1;
            segmentLine = null;
            clickParameter = 0d;

            if (polyline == null || polyline.Points.Count < 2)
                return false;

            segmentIndex = GetClosestSegmentIndex(polyline.Points, pickPoint, isClosed);
            if (segmentIndex < 0)
                return false;

            var endPoint = GetSegmentEndPoint(polyline, segmentIndex, isClosed);
            segmentLine = new Line(polyline.Points[segmentIndex], endPoint) { Thickness = polyline.Thickness };
            clickParameter = ProjectParameter(segmentLine, pickPoint);
            return true;
        }

        private static bool TryResolveEditableSegment(Polyline polyline, Point pickPoint, out bool startSegment, out Line segmentLine, out double clickParameter)
        {
            startSegment = false;
            int segmentIndex;
            if (!TryResolveClosestSegment(polyline, pickPoint, false, out segmentIndex, out segmentLine, out clickParameter))
                return false;

            if (clickParameter <= 0.5d)
            {
                startSegment = true;
            }
            else if (clickParameter > 0.5d)
            {
                startSegment = false;
            }

            return true;
        }

        private static Polyline BuildPath(Polyline source, double startParam, double endParam, bool includeStart, bool includeEnd)
        {
            var points = GetPolylinePoints(source);
            if (points.Count < 2)
                return null;

            var result = new List<Point>();
            AddPoint(result, GetPointAtParameter(points, startParam));

            int startIndex = (int)Math.Floor(startParam + Epsilon);
            int endIndex = (int)Math.Floor(endParam - Epsilon);
            for (int i = startIndex + 1; i <= endIndex; i++)
            {
                if (i >= 0 && i < points.Count)
                    AddPoint(result, points[i]);
            }

            AddPoint(result, GetPointAtParameter(points, endParam));
            return result.Count >= 2 ? new Polyline(result) { Thickness = source.Thickness } : null;
        }

        private static Polyline BuildClosedPath(Polyline source, double startParam, double endParam)
        {
            var points = GetRingPoints(source);
            if (points.Count < 3)
                return null;

            double total = points.Count;
            if (endParam < startParam)
                endParam += total;

            var result = new List<Point> { GetPointAtClosedParameter(points, startParam) };
            int startIndex = (int)Math.Floor(startParam + Epsilon);
            int endIndex = (int)Math.Floor(endParam - Epsilon);
            for (int i = startIndex + 1; i <= endIndex; i++)
                AddPoint(result, points[i % points.Count]);
            AddPoint(result, GetPointAtClosedParameter(points, endParam));
            return result.Count >= 2 ? new Polyline(result) { Thickness = source.Thickness } : null;
        }

        private static Polyline CreateEndpointPolyline(Polyline source, bool replaceStart, Point replacementPoint)
        {
            var points = source.Points.ToList();
            if (replaceStart)
                points[0] = replacementPoint;
            else
                points[points.Count - 1] = replacementPoint;

            return new Polyline(points) { Thickness = source.Thickness };
        }

        private static int GetClosestSegmentIndex(IReadOnlyList<Point> points, Point pickPoint, bool isClosed)
        {
            int bestIndex = -1;
            double bestDistanceSq = double.MaxValue;
            int segmentCount = isClosed ? GetRingPointsFromPoints(points).Count : points.Count - 1;
            for (int i = 0; i < segmentCount; i++)
            {
                var ringPoints = isClosed ? GetRingPointsFromPoints(points) : null;
                var segmentEnd = isClosed ? ringPoints[(i + 1) % ringPoints.Count] : points[i + 1];
                var closestPoint = GetClosestPointOnSegment(points[i], segmentEnd, pickPoint);
                var distanceSq = (closestPoint - pickPoint).LengthSquared;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestIndex = i;
            }

            return bestIndex;
        }

        private static Point GetSegmentEndPoint(Polyline polyline, int segmentIndex, bool isClosed)
        {
            if (!isClosed)
                return polyline.Points[segmentIndex + 1];

            var ring = GetRingPoints(polyline);
            return ring[(segmentIndex + 1) % ring.Count];
        }

        private static IReadOnlyList<Point> GetRingPoints(Polyline polyline)
        {
            if (polyline.Points.Count < 2)
                return Array.Empty<Point>();

            if (PointsEqual(polyline.Points[0], polyline.Points[polyline.Points.Count - 1]))
                return polyline.Points.Take(polyline.Points.Count - 1).ToList();

            return polyline.Points.ToList();
        }

        private static bool IsClosed(Polyline polyline)
        {
            return polyline.Points.Count > 2 && PointsEqual(polyline.Points[0], polyline.Points[polyline.Points.Count - 1]);
        }

        private static IReadOnlyList<Point> GetRingPointsFromPoints(IReadOnlyList<Point> points)
        {
            if (points == null || points.Count < 2)
                return Array.Empty<Point>();

            if (PointsEqual(points[0], points[points.Count - 1]))
                return points.Take(points.Count - 1).ToList();

            return points.ToList();
        }

        private static bool PointsEqual(Point first, Point second)
        {
            return Math.Abs(first.X - second.X) <= Epsilon && Math.Abs(first.Y - second.Y) <= Epsilon;
        }

        private static IReadOnlyList<Point> GetPolylinePoints(Polyline polyline)
        {
            if (polyline == null)
                return Array.Empty<Point>();

            return polyline.Points.ToList();
        }

        private static double GetTotalParameter(Polyline polyline, bool isClosed)
        {
            return isClosed ? GetRingPoints(polyline).Count : polyline.Points.Count - 1;
        }

        private static void AddPoint(List<Point> points, Point point)
        {
            if (points.Count == 0 || !PointsEqual(points[points.Count - 1], point))
                points.Add(point);
        }

        private static Point GetPointAtParameter(IReadOnlyList<Point> points, double parameter)
        {
            if (points.Count == 0)
                return new Point();

            if (parameter <= 0d)
                return points[0];

            int index = (int)Math.Floor(parameter);
            if (index >= points.Count - 1)
                return points[points.Count - 1];

            double t = parameter - index;
            var a = points[index];
            var b = points[index + 1];
            return new Point(a.X + ((b.X - a.X) * t), a.Y + ((b.Y - a.Y) * t));
        }

        private static Point GetPointAtClosedParameter(IReadOnlyList<Point> points, double parameter)
        {
            if (points.Count == 0)
                return new Point();

            double total = points.Count;
            while (parameter < 0d) parameter += total;
            while (parameter > total) parameter -= total;

            int index = (int)Math.Floor(parameter);
            double t = parameter - index;
            var a = points[index % points.Count];
            var b = points[(index + 1) % points.Count];
            return new Point(a.X + ((b.X - a.X) * t), a.Y + ((b.Y - a.Y) * t));
        }

        private static IntersectionPoint GetPreviousIntersection(List<IntersectionPoint> intersections, double clickedParameter, bool isClosed, double totalParameter)
        {
            if (!isClosed)
                return intersections.LastOrDefault(item => item.Parameter < clickedParameter - Epsilon);

            var wrapped = intersections
                .Select(item => new IntersectionPoint(item.Point, item.Parameter <= clickedParameter ? item.Parameter : item.Parameter - totalParameter))
                .OrderBy(item => item.Parameter)
                .LastOrDefault(item => item.Parameter < clickedParameter - Epsilon);
            return wrapped;
        }

        private static IntersectionPoint GetNextIntersection(List<IntersectionPoint> intersections, double clickedParameter, bool isClosed, double totalParameter)
        {
            if (!isClosed)
                return intersections.FirstOrDefault(item => item.Parameter > clickedParameter + Epsilon);

            var wrapped = intersections
                .Select(item => new IntersectionPoint(item.Point, item.Parameter < clickedParameter ? item.Parameter + totalParameter : item.Parameter))
                .OrderBy(item => item.Parameter)
                .FirstOrDefault(item => item.Parameter > clickedParameter + Epsilon);
            return wrapped;
        }

        private static List<IntersectionPoint> GetPolylineIntersections(Polyline polyline, IReadOnlyList<Entity> boundaries)
        {
            var points = GetRingPointsFromPoints(polyline.Points);
            bool closed = IsClosed(polyline);
            int segmentCount = closed ? points.Count : polyline.Points.Count - 1;
            var intersections = new List<IntersectionPoint>();

            for (int i = 0; i < segmentCount; i++)
            {
                var a = polyline.Points[i];
                var b = closed ? points[(i + 1) % points.Count] : polyline.Points[i + 1];
                var segmentLine = new Line(a, b) { Thickness = polyline.Thickness };
                var segInters = boundaries
                    .Where(TrimExtendSupport.IsSupportedBoundary)
                    .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(segmentLine, boundary))
                    .Select(item => new IntersectionPoint(item.Point, i + item.Parameter))
                    .Where(item => item.Parameter >= -Epsilon)
                    .ToList();

                intersections.AddRange(segInters);
            }

            return intersections
                .ToList();
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

        private static Point GetClosestPointOnSegment(Point start, Point end, Point point)
        {
            Vector direction = end - start;
            double lengthSq = direction.LengthSquared;
            if (lengthSq <= Epsilon)
                return start;

            double t = Vector.Multiply(point - start, direction) / lengthSq;
            t = Math.Max(0d, Math.Min(1d, t));
            return start + (direction * t);
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

    }
}
