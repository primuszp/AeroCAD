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
            return target is Polyline polyline &&
                polyline.Points.Count >= 2 &&
                boundaries.Any(IsSupportedBoundary);
        }

        public bool CanExtend(IReadOnlyList<Entity> boundaries, Entity target)
        {
            return target is Polyline polyline &&
                polyline.Points.Count >= 2 &&
                boundaries.Any(IsSupportedBoundary);
        }

        public Entity CreateTrimmed(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var polyline = target as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return null;

            if (!TryResolveClosestSegment(polyline, pickPoint, out int segmentIndex, out var segmentLine, out double clickParameter))
                return null;

            var intersections = boundaries
                .Where(IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(segmentLine, boundary))
                .Where(item => item.Parameter > Epsilon && item.Parameter < 1d - Epsilon)
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return null;

            Point? replacement = ResolveTrimPoint(intersections, clickParameter, out bool keepSuffix);

            if (!replacement.HasValue)
                return null;

            return CreateTrimmedPolyline(polyline, segmentIndex, keepSuffix, replacement.Value);
        }

        public Entity CreateExtended(IReadOnlyList<Entity> boundaries, Entity target, Point pickPoint)
        {
            var polyline = target as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return null;

            if (!TryResolveEditableSegment(polyline, pickPoint, out bool extendStart, out var segmentLine, out double clickParameter))
                return null;

            var intersections = boundaries
                .Where(IsSupportedBoundary)
                .SelectMany(boundary => TrimExtendGeometry.GetLineBoundaryIntersections(segmentLine, boundary))
                .OrderBy(item => item.Parameter)
                .ToList();

            if (intersections.Count == 0)
                return null;

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

            return replacement.HasValue ? CreateEndpointPolyline(polyline, extendStart, replacement.Value) : null;
        }

        private static bool TryResolveClosestSegment(Polyline polyline, Point pickPoint, out int segmentIndex, out Line segmentLine, out double clickParameter)
        {
            segmentIndex = -1;
            segmentLine = null;
            clickParameter = 0d;

            if (polyline == null || polyline.Points.Count < 2)
                return false;

            segmentIndex = GetClosestSegmentIndex(polyline.Points, pickPoint);
            if (segmentIndex < 0)
                return false;

            segmentLine = new Line(polyline.Points[segmentIndex], polyline.Points[segmentIndex + 1]) { Thickness = polyline.Thickness };
            clickParameter = ProjectParameter(segmentLine, pickPoint);
            return true;
        }

        private static bool TryResolveEditableSegment(Polyline polyline, Point pickPoint, out bool startSegment, out Line segmentLine, out double clickParameter)
        {
            startSegment = false;
            int segmentIndex;
            if (!TryResolveClosestSegment(polyline, pickPoint, out segmentIndex, out segmentLine, out clickParameter))
                return false;

            if (segmentIndex == 0)
            {
                startSegment = true;
            }
            else if (segmentIndex == polyline.Points.Count - 2)
            {
                startSegment = false;
            }
            else
            {
                return false;
            }

            return true;
        }

        private static Point? ResolveTrimPoint(IReadOnlyList<LineIntersectionPoint> intersections, double clickParameter, out bool keepSuffix)
        {
            keepSuffix = false;

            var before = intersections
                .Where(item => item.Parameter < clickParameter - Epsilon)
                .OrderByDescending(item => item.Parameter)
                .FirstOrDefault();
            if (before != null)
                return before.Point;

            var after = intersections
                .Where(item => item.Parameter > clickParameter + Epsilon)
                .OrderBy(item => item.Parameter)
                .FirstOrDefault();
            if (after != null)
            {
                keepSuffix = true;
                return after.Point;
            }

            return null;
        }

        private static Polyline CreateTrimmedPolyline(Polyline source, int segmentIndex, bool keepSuffix, Point replacementPoint)
        {
            var points = source.Points.ToList();
            List<Point> result;
            if (keepSuffix)
            {
                result = new List<Point> { replacementPoint };
                for (int i = segmentIndex + 1; i < points.Count; i++)
                    result.Add(points[i]);
            }
            else
            {
                result = new List<Point>();
                for (int i = 0; i <= segmentIndex; i++)
                    result.Add(points[i]);
                result.Add(replacementPoint);
            }

            if (result.Count < 2)
                return null;

            return new Polyline(result) { Thickness = source.Thickness };
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

        private static int GetClosestSegmentIndex(IReadOnlyList<Point> points, Point pickPoint)
        {
            int bestIndex = -1;
            double bestDistanceSq = double.MaxValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                var closestPoint = GetClosestPointOnSegment(points[i], points[i + 1], pickPoint);
                var distanceSq = (closestPoint - pickPoint).LengthSquared;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestIndex = i;
            }

            return bestIndex;
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

        private static bool IsSupportedBoundary(Entity boundary)
        {
            return boundary is Line || boundary is Circle || boundary is Polyline || boundary is Arc || boundary is Rectangle;
        }
    }
}
