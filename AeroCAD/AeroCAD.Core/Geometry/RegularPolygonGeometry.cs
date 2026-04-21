using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Primusz.AeroCAD.Core.GeometryMath
{
    public static class RegularPolygonGeometry
    {
        private const double Epsilon = 1e-9;

        public static IReadOnlyList<Point> BuildClosedPolygon(Point center, int sides, double radius, double rotationOffset, bool inscribed)
        {
            if (sides < 3 || sides > 1024)
                return Array.Empty<Point>();

            if (radius <= Epsilon)
                return Array.Empty<Point>();

            double vertexRadius = inscribed
                ? radius
                : radius / Math.Cos(Math.PI / sides);

            if (double.IsNaN(vertexRadius) || double.IsInfinity(vertexRadius) || vertexRadius <= Epsilon)
                return Array.Empty<Point>();

            var points = new List<Point>(sides + 1);
            double step = (Math.PI * 2d) / sides;
            for (int i = 0; i < sides; i++)
            {
                double angle = rotationOffset + (i * step);
                points.Add(CircularGeometry.GetPoint(center, vertexRadius, angle));
            }

            points.Add(points[0]);
            return points;
        }

        public static Point[] BuildSidePolygon(Point firstEdgePoint, Point secondEdgePoint, int sides, out Point center, out double rotationOffset)
        {
            center = default(Point);
            rotationOffset = 0d;

            if (sides < 3 || sides > 1024)
                return Array.Empty<Point>();

            Vector edge = secondEdgePoint - firstEdgePoint;
            double edgeLength = edge.Length;
            if (edgeLength <= Epsilon)
                return Array.Empty<Point>();

            double radius = edgeLength / (2d * Math.Sin(Math.PI / sides));
            if (radius <= Epsilon)
                return Array.Empty<Point>();

            double apothem = edgeLength / (2d * Math.Tan(Math.PI / sides));
            Vector normal = new Vector(-edge.Y, edge.X);
            normal.Normalize();

            var midpoint = firstEdgePoint + (edge * 0.5d);
            center = midpoint + (normal * apothem);

            rotationOffset = Math.Atan2(firstEdgePoint.Y - center.Y, firstEdgePoint.X - center.X);
            return BuildClosedPolygon(center, sides, radius, rotationOffset, inscribed: true).ToArray();
        }
    }
}
