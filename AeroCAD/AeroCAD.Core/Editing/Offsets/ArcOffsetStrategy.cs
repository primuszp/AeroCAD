using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    public class ArcOffsetStrategy : IEntityOffsetStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Arc;
        }

        public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint)
        {
            var arc = entity as Arc;
            if (arc == null)
                return null;

            double newRadius = (throughPoint - arc.Center).Length;
            return newRadius > double.Epsilon
                ? new Arc(arc.Center, newRadius, arc.StartAngle, arc.SweepAngle) { Thickness = arc.Thickness }
                : null;
        }

        public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint)
        {
            var arc = entity as Arc;
            if (arc == null)
                return null;

            double sideRadius = (sidePoint - arc.Center).Length;
            double newRadius = sideRadius >= arc.Radius
                ? arc.Radius + Math.Abs(distance)
                : arc.Radius - Math.Abs(distance);

            return newRadius > double.Epsilon
                ? new Arc(arc.Center, newRadius, arc.StartAngle, arc.SweepAngle) { Thickness = arc.Thickness }
                : null;
        }
    }
}
