using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Editing.TrimExtend
{
    internal static class TrimExtendGeometry
    {
        private const double Epsilon = 1e-9;

        public static IReadOnlyList<LineIntersectionPoint> GetLineBoundaryIntersections(Line target, Entity boundary)
        {
            return GetLineBoundaryIntersections(target, boundary, restrictTargetToSegment: true);
        }

        public static IReadOnlyList<LineIntersectionPoint> GetLineBoundaryIntersections(Line target, Entity boundary, bool restrictTargetToSegment)
        {
            if (boundary is Line boundaryLine)
                return GetLineLineIntersections(target, boundaryLine, restrictTargetToSegment);

            if (boundary is Circle boundaryCircle)
                return GetLineCircleIntersections(target, boundaryCircle, restrictTargetToSegment);

            if (boundary is Polyline boundaryPolyline)
                return GetPolylineLineIntersections(target, boundaryPolyline, restrictTargetToSegment);

            if (boundary is Arc boundaryArc)
            {
                return GetLineCircleIntersections(target, new Circle(boundaryArc.Center, boundaryArc.Radius), restrictTargetToSegment)
                    .Where(item => CircularGeometry.IsAngleOnArc(
                        CircularGeometry.GetAngle(boundaryArc.Center, item.Point),
                        boundaryArc.StartAngle,
                        boundaryArc.SweepAngle))
                    .ToList();
            }

            if (boundary is Rectangle boundaryRect)
                return GetRectangleLineIntersections(target, boundaryRect, restrictTargetToSegment);

            if (boundary is ITrimExtendBoundaryGeometry boundaryGeometry)
                return boundaryGeometry.GetLineIntersections(target, restrictTargetToSegment) ?? Array.Empty<LineIntersectionPoint>();

            return Array.Empty<LineIntersectionPoint>();
        }

        public static IReadOnlyList<CircularIntersectionPoint> GetCircularBoundaryIntersections(Point center, double radius, Entity boundary)
        {
            if (boundary is Line boundaryLine)
            {
                return GetLineCircleIntersections(boundaryLine, new Circle(center, radius), restrictTargetToSegment: true)
                    .Select(item => new CircularIntersectionPoint(item.Point, CircularGeometry.GetAngle(center, item.Point)))
                    .ToList();
            }

            if (boundary is Circle boundaryCircle)
                return GetCircleCircleIntersections(center, radius, boundaryCircle.Center, boundaryCircle.Radius);

            if (boundary is Polyline boundaryPolyline)
                return GetPolylineCircleIntersections(center, radius, boundaryPolyline);

            if (boundary is Arc boundaryArc)
            {
                return GetCircleCircleIntersections(center, radius, boundaryArc.Center, boundaryArc.Radius)
                    .Where(item => CircularGeometry.IsAngleOnArc(
                        CircularGeometry.GetAngle(boundaryArc.Center, item.Point),
                        boundaryArc.StartAngle,
                        boundaryArc.SweepAngle))
                    .ToList();
            }

            if (boundary is Rectangle boundaryRect)
                return GetRectangleCircleIntersections(center, radius, boundaryRect);

            if (boundary is ITrimExtendBoundaryGeometry boundaryGeometry)
                return boundaryGeometry.GetCircularIntersections(center, radius) ?? Array.Empty<CircularIntersectionPoint>();

            return Array.Empty<CircularIntersectionPoint>();
        }

        private static IReadOnlyList<LineIntersectionPoint> GetPolylineLineIntersections(Line target, Polyline boundary, bool restrictTargetToSegment)
        {
            var intersections = new List<LineIntersectionPoint>();
            if (boundary?.Points == null || boundary.Points.Count < 2)
                return intersections;

            for (int i = 0; i < boundary.Points.Count - 1; i++)
            {
                var boundarySegment = new Line(boundary.Points[i], boundary.Points[i + 1]);
                intersections.AddRange(GetLineLineIntersections(target, boundarySegment, restrictTargetToSegment));
            }

            return TrimExtendIntersectionDeduper.ByParameter(intersections, item => item.Parameter, 1e-9).ToList();
        }

        private static IReadOnlyList<CircularIntersectionPoint> GetPolylineCircleIntersections(Point center, double radius, Polyline boundary)
        {
            return GetPolylineCircleIntersections(center, radius, boundary, restrictTargetToSegment: true);
        }

        private static IReadOnlyList<CircularIntersectionPoint> GetPolylineCircleIntersections(Point center, double radius, Polyline boundary, bool restrictTargetToSegment)
        {
            var intersections = new List<CircularIntersectionPoint>();
            if (boundary?.Points == null || boundary.Points.Count < 2)
                return intersections;

            var circle = new Circle(center, radius);
            for (int i = 0; i < boundary.Points.Count - 1; i++)
            {
                var segment = new Line(boundary.Points[i], boundary.Points[i + 1]);
                intersections.AddRange(
                    GetLineCircleIntersections(segment, circle, restrictTargetToSegment)
                        .Select(item => new CircularIntersectionPoint(item.Point, CircularGeometry.GetAngle(center, item.Point))));
            }

            return TrimExtendIntersectionDeduper.ByAngle(intersections, item => item.Angle, 1e-9).ToList();
        }

        private static IReadOnlyList<LineIntersectionPoint> GetLineLineIntersections(Line target, Line boundary, bool restrictTargetToSegment)
        {
            Vector r = target.EndPoint - target.StartPoint;
            Vector s = boundary.EndPoint - boundary.StartPoint;
            double denominator = Cross(r, s);

            if (Math.Abs(denominator) <= Epsilon)
                return Array.Empty<LineIntersectionPoint>();

            Vector qp = boundary.StartPoint - target.StartPoint;
            double t = Cross(qp, s) / denominator;
            double u = Cross(qp, r) / denominator;

            if (restrictTargetToSegment && (t < -Epsilon || t > 1d + Epsilon))
                return Array.Empty<LineIntersectionPoint>();

            if (u < -Epsilon || u > 1d + Epsilon)
                return Array.Empty<LineIntersectionPoint>();

            return new[]
            {
                new LineIntersectionPoint(target.StartPoint + (r * t), t)
            };
        }

        private static IReadOnlyList<LineIntersectionPoint> GetLineCircleIntersections(Line target, Circle boundary)
        {
            return GetLineCircleIntersections(target, boundary, restrictTargetToSegment: true);
        }

        private static IReadOnlyList<LineIntersectionPoint> GetLineCircleIntersections(Line target, Circle boundary, bool restrictTargetToSegment)
        {
            Vector d = target.EndPoint - target.StartPoint;
            Vector f = target.StartPoint - boundary.Center;

            double a = d.X * d.X + d.Y * d.Y;
            if (a <= Epsilon)
                return Array.Empty<LineIntersectionPoint>();

            double b = 2d * ((f.X * d.X) + (f.Y * d.Y));
            double c = (f.X * f.X) + (f.Y * f.Y) - (boundary.Radius * boundary.Radius);
            double discriminant = (b * b) - (4d * a * c);

            if (discriminant < -Epsilon)
                return Array.Empty<LineIntersectionPoint>();

            if (discriminant < 0d)
                discriminant = 0d;

            double sqrt = Math.Sqrt(discriminant);
            double t1 = (-b - sqrt) / (2d * a);
            double t2 = (-b + sqrt) / (2d * a);

            var intersections = new List<LineIntersectionPoint>
            {
                new LineIntersectionPoint(target.StartPoint + (d * t1), t1)
            };

            if (Math.Abs(t2 - t1) > Epsilon)
                intersections.Add(new LineIntersectionPoint(target.StartPoint + (d * t2), t2));

            return restrictTargetToSegment
                ? intersections.Where(item => item.Parameter >= -Epsilon && item.Parameter <= 1d + Epsilon).ToList()
                : intersections;
        }

        private static IReadOnlyList<CircularIntersectionPoint> GetCircleCircleIntersections(
            Point firstCenter,
            double firstRadius,
            Point secondCenter,
            double secondRadius)
        {
            Vector delta = secondCenter - firstCenter;
            double distance = delta.Length;
            double radiusSum = firstRadius + secondRadius;
            double radiusDiff = Math.Abs(firstRadius - secondRadius);
            double relTol = Epsilon * (firstRadius + secondRadius + 1d);

            if (distance <= relTol
                || distance > radiusSum + relTol
                || distance < radiusDiff - relTol)
                return Array.Empty<CircularIntersectionPoint>();

            double a = ((firstRadius * firstRadius) - (secondRadius * secondRadius) + (distance * distance)) / (2d * distance);
            double hSq = (firstRadius * firstRadius) - (a * a);
            if (hSq < -relTol * firstRadius)
                return Array.Empty<CircularIntersectionPoint>();

            if (hSq < 0d)
                hSq = 0d;

            double h = Math.Sqrt(hSq);
            Vector direction = delta;
            direction.Normalize();

            Point midpoint = firstCenter + (direction * a);
            Vector offset = new Vector(-direction.Y * h, direction.X * h);

            var firstPoint = midpoint + offset;
            var intersections = new List<CircularIntersectionPoint>
            {
                new CircularIntersectionPoint(firstPoint, CircularGeometry.GetAngle(firstCenter, firstPoint))
            };

            if (h > relTol)
            {
                var secondPoint = midpoint - offset;
                intersections.Add(new CircularIntersectionPoint(secondPoint, CircularGeometry.GetAngle(firstCenter, secondPoint)));
            }

            return intersections;
        }

        private static IReadOnlyList<LineIntersectionPoint> GetRectangleLineIntersections(Line target, Rectangle rect, bool restrictTargetToSegment)
        {
            var corners = GetRectangleCorners(rect);
            var intersections = new List<LineIntersectionPoint>();
            for (int i = 0; i < 4; i++)
            {
                var segment = new Line(corners[i], corners[(i + 1) % 4]);
                intersections.AddRange(GetLineLineIntersections(target, segment, restrictTargetToSegment));
            }
            return TrimExtendIntersectionDeduper.ByParameter(intersections, item => item.Parameter, 1e-9).ToList();
        }

        private static IReadOnlyList<CircularIntersectionPoint> GetRectangleCircleIntersections(Point center, double radius, Rectangle rect)
        {
            return GetRectangleCircleIntersections(center, radius, rect, restrictTargetToSegment: true);
        }

        private static IReadOnlyList<CircularIntersectionPoint> GetRectangleCircleIntersections(Point center, double radius, Rectangle rect, bool restrictTargetToSegment)
        {
            var corners = GetRectangleCorners(rect);
            var circle = new Circle(center, radius);
            var intersections = new List<CircularIntersectionPoint>();
            for (int i = 0; i < 4; i++)
            {
                var segment = new Line(corners[i], corners[(i + 1) % 4]);
                intersections.AddRange(
                    GetLineCircleIntersections(segment, circle, restrictTargetToSegment)
                        .Select(item => new CircularIntersectionPoint(item.Point, CircularGeometry.GetAngle(center, item.Point))));
            }
            return TrimExtendIntersectionDeduper.ByAngle(intersections, item => item.Angle, 1e-9).ToList();
        }

        private static Point[] GetRectangleCorners(Rectangle rect)
        {
            return new[]
            {
                rect.TopLeft,
                new Point(rect.BottomRight.X, rect.TopLeft.Y),
                rect.BottomRight,
                new Point(rect.TopLeft.X, rect.BottomRight.Y)
            };
        }

        private static double Cross(Vector first, Vector second)
        {
            return (first.X * second.Y) - (first.Y * second.X);
        }
    }

    public sealed class LineIntersectionPoint
    {
        public LineIntersectionPoint(Point point, double parameter)
        {
            Point = point;
            Parameter = parameter;
        }

        public Point Point { get; }

        public double Parameter { get; }
    }

    public sealed class CircularIntersectionPoint
    {
        public CircularIntersectionPoint(Point point, double angle)
        {
            Point = point;
            Angle = angle;
        }

        public Point Point { get; }

        public double Angle { get; }
    }
}
