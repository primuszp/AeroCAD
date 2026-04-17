using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.GeometryMath;

namespace Primusz.AeroCAD.Core.Spatial
{
    public class ArcBoundsStrategy : IEntityBoundsStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Arc;
        }

        public Rect GetBounds(Entity entity)
        {
            var arc = entity as Arc;
            if (arc == null || arc.Radius <= 0d || System.Math.Abs(arc.SweepAngle) <= double.Epsilon)
                return Rect.Empty;

            var points = new List<Point>
            {
                arc.StartPoint,
                arc.EndPoint
            };

            AddIfOnArc(points, arc, 0d);
            AddIfOnArc(points, arc, System.Math.PI / 2d);
            AddIfOnArc(points, arc, System.Math.PI);
            AddIfOnArc(points, arc, (System.Math.PI * 3d) / 2d);

            double minX = points[0].X;
            double maxX = points[0].X;
            double minY = points[0].Y;
            double maxY = points[0].Y;
            foreach (var point in points)
            {
                if (point.X < minX) minX = point.X;
                if (point.X > maxX) maxX = point.X;
                if (point.Y < minY) minY = point.Y;
                if (point.Y > maxY) maxY = point.Y;
            }

            var bounds = new Rect(new Point(minX, minY), new Point(maxX, maxY));
            double margin = System.Math.Max(1d, arc.Thickness + 4d) * System.Math.Max(arc.Scale, 1e-6);
            bounds.Inflate(margin, margin);
            return bounds;
        }

        private static void AddIfOnArc(List<Point> points, Arc arc, double angle)
        {
            if (CircularGeometry.IsAngleOnArc(angle, arc.StartAngle, arc.SweepAngle))
                points.Add(CircularGeometry.GetPoint(arc.Center, arc.Radius, angle));
        }
    }
}
