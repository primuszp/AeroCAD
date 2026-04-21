using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    internal static class PolylinePathOperations
    {
        private const double Epsilon = 1e-9;

        public static bool IsClosed(Polyline polyline)
        {
            return polyline != null
                && polyline.Points.Count > 2
                && PointsEqual(polyline.Points[0], polyline.Points[polyline.Points.Count - 1]);
        }

        public static IReadOnlyList<Point> GetRingPoints(Polyline polyline)
        {
            if (polyline == null || polyline.Points.Count < 2)
                return Array.Empty<Point>();

            return IsClosed(polyline)
                ? polyline.Points.Take(polyline.Points.Count - 1).ToList()
                : polyline.Points.ToList();
        }

        public static int GetClosestSegmentIndex(IReadOnlyList<Point> points, Point pickPoint, bool closed)
        {
            if (points == null || points.Count < 2)
                return -1;

            var ringPoints = closed ? GetRingPointsFromPoints(points) : points;
            int bestIndex = -1;
            double bestDistanceSq = double.MaxValue;
            int segmentCount = closed ? ringPoints.Count : points.Count - 1;

            for (int i = 0; i < segmentCount; i++)
            {
                var segmentEnd = closed ? ringPoints[(i + 1) % ringPoints.Count] : points[i + 1];
                var closestPoint = GetClosestPointOnSegment(points[i], segmentEnd, pickPoint);
                var distanceSq = (closestPoint - pickPoint).LengthSquared;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                bestIndex = i;
            }

            return bestIndex;
        }

        public static Point GetSegmentEndPoint(Polyline polyline, int segmentIndex, bool closed)
        {
            if (!closed)
                return polyline.Points[segmentIndex + 1];

            var ring = GetRingPoints(polyline);
            return ring[(segmentIndex + 1) % ring.Count];
        }

        public static Polyline BuildOpenPath(Polyline source, double startParam, double endParam)
        {
            var points = GetPoints(source);
            if (points == null || points.Count < 2)
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
            return CreatePolyline(source, result);
        }

        public static Polyline BuildClosedPath(Polyline source, double startParam, double endParam)
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
            return CreatePolyline(source, result);
        }

        public static IReadOnlyList<Polyline> BuildClosedSplitPaths(Polyline source, double startParam, double endParam)
        {
            var points = GetRingPoints(source);
            if (points.Count < 3)
                return Array.Empty<Polyline>();

            double total = points.Count;
            if (endParam < startParam)
                endParam += total;

            var first = BuildClosedSidePath(source, points, startParam, endParam);
            var second = BuildClosedSidePath(source, points, endParam, startParam + total);

            return new[] { first, second }.Where(item => item != null).ToList();
        }

        public static Polyline ReplaceEndpoint(Polyline source, bool replaceStart, Point replacementPoint)
        {
            var points = GetPoints(source);
            if (points == null || points.Count < 2)
                return null;

            if (replaceStart)
                points[0] = replacementPoint;
            else
                points[points.Count - 1] = replacementPoint;

            return new Polyline(points) { Thickness = source.Thickness };
        }

        private static List<Point> GetPoints(Polyline polyline)
        {
            return polyline?.Points?.ToList();
        }

        private static Polyline CreatePolyline(Polyline source, List<Point> points)
        {
            return points.Count >= 2 ? new Polyline(points) { Thickness = source.Thickness } : null;
        }

        private static Polyline BuildClosedSidePath(Polyline source, IReadOnlyList<Point> points, double startParam, double endParam)
        {
            var result = new List<Point> { GetPointAtClosedParameter(points, startParam) };
            int startIndex = (int)Math.Floor(startParam + Epsilon);
            int endIndex = (int)Math.Floor(endParam - Epsilon);
            for (int i = startIndex + 1; i <= endIndex; i++)
                AddPoint(result, points[i % points.Count]);
            AddPoint(result, GetPointAtClosedParameter(points, endParam));
            return CreatePolyline(source, result);
        }

        private static IReadOnlyList<Point> GetRingPointsFromPoints(IReadOnlyList<Point> points)
        {
            if (points == null || points.Count < 2)
                return Array.Empty<Point>();

            if (PointsEqual(points[0], points[points.Count - 1]))
                return points.Take(points.Count - 1).ToList();

            return points.ToList();
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

        private static void AddPoint(List<Point> points, Point point)
        {
            if (points.Count == 0 || !PointsEqual(points[points.Count - 1], point))
                points.Add(point);
        }

        private static bool PointsEqual(Point first, Point second)
        {
            return Math.Abs(first.X - second.X) <= Epsilon && Math.Abs(first.Y - second.Y) <= Epsilon;
        }
    }
}
