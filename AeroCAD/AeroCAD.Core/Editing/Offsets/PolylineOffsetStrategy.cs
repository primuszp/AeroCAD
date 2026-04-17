using System;
using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    public class PolylineOffsetStrategy : IEntityOffsetStrategy
    {
        private const double Epsilon = 1e-9;

        public bool CanHandle(Entity entity)
        {
            return entity is Polyline polyline && polyline.Points.Count >= 2;
        }

        public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint)
        {
            var polyline = entity as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return null;

            if (!TryGetNearestSegment(polyline.Points, throughPoint, out var segmentStart, out _, out var normal))
                return null;

            double signedDistance = Vector.Multiply(throughPoint - segmentStart, normal);
            return CreateOffsetPolyline(polyline, signedDistance);
        }

        public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint)
        {
            var polyline = entity as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return null;

            if (!TryGetNearestSegment(polyline.Points, sidePoint, out var segmentStart, out _, out var normal))
                return null;

            double signedDistance = Vector.Multiply(sidePoint - segmentStart, normal) >= 0d
                ? Math.Abs(distance)
                : -Math.Abs(distance);

            return CreateOffsetPolyline(polyline, signedDistance);
        }

        private static Polyline CreateOffsetPolyline(Polyline source, double signedDistance)
        {
            var offsetPoints = BuildOffsetPoints(source.Points, signedDistance);
            return offsetPoints == null || offsetPoints.Count < 2
                ? null
                : new Polyline(offsetPoints) { Thickness = source.Thickness };
        }

        private static IReadOnlyList<Point> BuildOffsetPoints(IReadOnlyList<Point> points, double signedDistance)
        {
            var segments = new List<OffsetSegment>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (!TryCreateOffsetSegment(points[i], points[i + 1], signedDistance, out var segment))
                    continue;

                segments.Add(segment);
            }

            if (segments.Count == 0)
                return null;

            var result = new List<Point> { segments[0].Start };
            for (int i = 1; i < segments.Count; i++)
            {
                if (TryIntersectLines(segments[i - 1].Start, segments[i - 1].End, segments[i].Start, segments[i].End, out var intersection))
                    result.Add(intersection);
                else
                    result.Add(segments[i].Start);
            }

            result.Add(segments[segments.Count - 1].End);
            return result;
        }

        private static bool TryGetNearestSegment(
            IReadOnlyList<Point> points,
            Point referencePoint,
            out Point segmentStart,
            out Point segmentEnd,
            out Vector normal)
        {
            segmentStart = default(Point);
            segmentEnd = default(Point);
            normal = default(Vector);

            double bestDistanceSq = double.MaxValue;
            for (int i = 0; i < points.Count - 1; i++)
            {
                var start = points[i];
                var end = points[i + 1];
                var direction = end - start;
                if (direction.LengthSquared <= Epsilon)
                    continue;

                direction.Normalize();
                var segmentNormal = new Vector(-direction.Y, direction.X);
                var closestPoint = GetClosestPointOnSegment(start, end, referencePoint);
                var distanceSq = (closestPoint - referencePoint).LengthSquared;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                segmentStart = start;
                segmentEnd = end;
                normal = segmentNormal;
            }

            return bestDistanceSq < double.MaxValue;
        }

        private static Point GetClosestPointOnSegment(Point start, Point end, Point point)
        {
            Vector direction = end - start;
            double lengthSq = direction.LengthSquared;
            if (lengthSq <= Epsilon)
                return start;

            var toPoint = point - start;
            double t = Vector.Multiply(toPoint, direction) / lengthSq;
            t = Math.Max(0d, Math.Min(1d, t));
            return start + (direction * t);
        }

        private static bool TryCreateOffsetSegment(Point start, Point end, double signedDistance, out OffsetSegment segment)
        {
            segment = default(OffsetSegment);
            Vector direction = end - start;
            if (direction.LengthSquared <= Epsilon)
                return false;

            direction.Normalize();
            Vector normal = new Vector(-direction.Y, direction.X);
            Vector offset = normal * signedDistance;
            segment = new OffsetSegment(start + offset, end + offset);
            return true;
        }

        private static bool TryIntersectLines(Point a1, Point a2, Point b1, Point b2, out Point intersection)
        {
            intersection = default(Point);

            Vector r = a2 - a1;
            Vector s = b2 - b1;
            double denominator = Cross(r, s);
            if (Math.Abs(denominator) <= Epsilon)
                return false;

            Vector qp = b1 - a1;
            double t = Cross(qp, s) / denominator;
            intersection = a1 + (r * t);
            return true;
        }

        private static double Cross(Vector first, Vector second)
        {
            return (first.X * second.Y) - (first.Y * second.X);
        }

        private readonly struct OffsetSegment
        {
            public OffsetSegment(Point start, Point end)
            {
                Start = start;
                End = end;
            }

            public Point Start { get; }

            public Point End { get; }
        }
    }
}
