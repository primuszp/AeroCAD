using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Primusz.AeroCAD.SamplePlugin
{
    public static class RoadPlanGeometryBuilder
    {
        public static Geometry BuildGeometry(IReadOnlyList<RoadPlanVertex> vertices)
        {
            var geometry = new StreamGeometry();
            if (vertices == null || vertices.Count == 0)
                return geometry;

            using (var context = geometry.Open())
            {
                Point current = vertices[0].Location;
                context.BeginFigure(current, false, false);

                for (int i = 1; i < vertices.Count; i++)
                {
                    if (i < vertices.Count - 1 && vertices[i].Radius > 0d && TryCreateFillet(vertices[i - 1].Location, vertices[i].Location, vertices[i + 1].Location, vertices[i].Radius, out var fillet))
                    {
                        context.LineTo(fillet.Start, true, false);
                        context.ArcTo(fillet.End, new Size(fillet.Radius, fillet.Radius), 0d, false, fillet.SweepDirection, true, false);
                        current = fillet.End;
                    }
                    else if ((vertices[i].Location - current).LengthSquared > 1e-9)
                    {
                        context.LineTo(vertices[i].Location, true, false);
                        current = vertices[i].Location;
                    }
                }
            }

            if (geometry.CanFreeze)
                geometry.Freeze();
            return geometry;
        }

        public static Geometry BuildTangentGeometry(IReadOnlyList<RoadPlanControlSegment> segments)
        {
            var geometry = new GeometryGroup();
            if (segments == null || segments.Count == 0)
                return geometry;

            foreach (var segment in segments)
            {
                var line = new LineGeometry(segment.Start, segment.End);
                if (line.CanFreeze)
                    line.Freeze();
                geometry.Children.Add(line);
            }

            if (geometry.CanFreeze)
                geometry.Freeze();
            return geometry;
        }

        public static IReadOnlyList<RoadPlanControlSegment> BuildControlSegments(IReadOnlyList<Point> controlPoints)
        {
            var segments = new List<RoadPlanControlSegment>();
            if (controlPoints == null || controlPoints.Count < 2)
                return segments;

            for (int i = 0; i < controlPoints.Count - 1; i++)
                segments.Add(new RoadPlanControlSegment(controlPoints[i], controlPoints[i + 1]));
            return segments;
        }

        public static IReadOnlyList<RoadPlanVertex> SolveVerticesFromSegments(IReadOnlyList<RoadPlanControlSegment> segments, IReadOnlyList<RoadPlanVertex> sourceVertices)
        {
            var solved = new List<RoadPlanVertex>();
            if (segments == null || segments.Count == 0)
                return solved;

            solved.Add(CreateVertex(segments[0].Start, sourceVertices, 0));

            for (int i = 0; i < segments.Count - 1; i++)
            {
                var current = segments[i];
                var next = segments[i + 1];
                Point location = current.End;
                if (TryIntersectLines(current.Start, current.End, next.Start, next.End, out var intersection))
                    location = intersection;

                var template = sourceVertices != null && i + 1 < sourceVertices.Count ? sourceVertices[i + 1] : null;
                solved.Add(CreateVertex(location, sourceVertices, i + 1));
            }

            solved.Add(CreateVertex(segments[segments.Count - 1].End, sourceVertices, solved.Count));
            return solved;
        }

        public static IReadOnlyList<RoadPlanVertex> SolveVertices(IReadOnlyList<Point> controlPoints, IReadOnlyList<RoadPlanVertex> sourceVertices)
        {
            return SolveVerticesFromSegments(BuildControlSegments(controlPoints), sourceVertices);
        }

        public static Rect GetBounds(IReadOnlyList<RoadPlanVertex> vertices)
        {
            var geometry = BuildGeometry(vertices);
            return geometry.Bounds;
        }

        public static Rect GetControlBounds(IReadOnlyList<RoadPlanControlSegment> segments)
        {
            var geometry = BuildTangentGeometry(segments);
            return geometry.Bounds;
        }

        private static bool TryCreateFillet(Point previous, Point vertex, Point next, double requestedRadius, out Fillet fillet)
        {
            fillet = default;
            Vector inDir = previous - vertex;
            Vector outDir = next - vertex;
            double inLength = inDir.Length;
            double outLength = outDir.Length;
            if (inLength < 1e-9 || outLength < 1e-9)
                return false;

            inDir.Normalize();
            outDir.Normalize();
            double dot = Math.Max(-0.999999, Math.Min(0.999999, inDir.X * outDir.X + inDir.Y * outDir.Y));
            double angle = Math.Acos(dot);
            if (angle < 1e-6 || Math.Abs(Math.PI - angle) < 1e-6)
                return false;

            double tangent = requestedRadius / Math.Tan(angle / 2d);
            tangent = Math.Min(tangent, Math.Min(inLength, outLength) * 0.45d);
            double radius = tangent * Math.Tan(angle / 2d);
            Point start = vertex + (inDir * tangent);
            Point end = vertex + (outDir * tangent);
            double cross = inDir.X * outDir.Y - inDir.Y * outDir.X;
            fillet = new Fillet(start, end, radius, cross < 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise);
            return radius > 1e-6;
        }

        private static bool TryIntersectLines(Point a1, Point a2, Point b1, Point b2, out Point intersection)
        {
            intersection = default;

            double x1 = a1.X;
            double y1 = a1.Y;
            double x2 = a2.X;
            double y2 = a2.Y;
            double x3 = b1.X;
            double y3 = b1.Y;
            double x4 = b2.X;
            double y4 = b2.Y;

            double denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(denominator) < 1e-9)
                return false;

            double det1 = x1 * y2 - y1 * x2;
            double det2 = x3 * y4 - y3 * x4;
            double px = (det1 * (x3 - x4) - (x1 - x2) * det2) / denominator;
            double py = (det1 * (y3 - y4) - (y1 - y2) * det2) / denominator;
            intersection = new Point(px, py);
            return true;
        }

        public static bool TryIntersectSupportLines(Point a1, Point a2, Point b1, Point b2, out Point intersection)
        {
            return TryIntersectLines(a1, a2, b1, b2, out intersection);
        }

        public static Point GetMidpoint(RoadPlanControlSegment segment)
        {
            return new Point((segment.Start.X + segment.End.X) / 2.0, (segment.Start.Y + segment.End.Y) / 2.0);
        }

        private static RoadPlanVertex CreateVertex(Point location, IReadOnlyList<RoadPlanVertex> sourceVertices, int index)
        {
            if (sourceVertices == null || index < 0 || index >= sourceVertices.Count)
                return new RoadPlanVertex(location);

            var source = sourceVertices[index];
            return new RoadPlanVertex(location, source.Radius, source.InTransition, source.OutTransition);
        }

        private static double GetRadius(IReadOnlyList<RoadPlanVertex> sourceVertices, int index)
        {
            if (sourceVertices == null || index < 0 || index >= sourceVertices.Count)
                return 0d;
            return sourceVertices[index].Radius;
        }

        private readonly struct Fillet
        {
            public Fillet(Point start, Point end, double radius, SweepDirection sweepDirection)
            {
                Start = start;
                End = end;
                Radius = radius;
                SweepDirection = sweepDirection;
            }

            public Point Start { get; }

            public Point End { get; }

            public double Radius { get; }

            public SweepDirection SweepDirection { get; }
        }
    }
}
