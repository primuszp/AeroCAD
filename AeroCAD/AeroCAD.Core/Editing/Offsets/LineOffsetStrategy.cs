using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    public class LineOffsetStrategy : IEntityOffsetStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Line;
        }

        public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint)
        {
            var line = entity as Line;
            if (line == null)
                return null;

            Vector direction = line.EndPoint - line.StartPoint;
            double length = direction.Length;
            if (length <= double.Epsilon)
                return null;

            Vector normal = new Vector(-direction.Y / length, direction.X / length);
            Vector fromStart = throughPoint - line.StartPoint;
            double signedDistance = Vector.Multiply(fromStart, normal);

            return CreateOffsetLine(line, signedDistance, normal);
        }

        public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint)
        {
            var line = entity as Line;
            if (line == null)
                return null;

            Vector direction = line.EndPoint - line.StartPoint;
            double length = direction.Length;
            if (length <= double.Epsilon)
                return null;

            Vector normal = new Vector(-direction.Y / length, direction.X / length);
            Vector fromStart = sidePoint - line.StartPoint;
            double signedDistance = Vector.Multiply(fromStart, normal) >= 0d
                ? Math.Abs(distance)
                : -Math.Abs(distance);

            return CreateOffsetLine(line, signedDistance, normal);
        }

        private static Entity CreateOffsetLine(Line source, double signedDistance, Vector normal)
        {
            if (Math.Abs(signedDistance) <= double.Epsilon)
                return source.Duplicate();

            Vector offset = normal * signedDistance;
            return new Line(source.StartPoint + offset, source.EndPoint + offset)
            {
                Thickness = source.Thickness
            };
        }
    }
}
