using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    public class CircleOffsetStrategy : IEntityOffsetStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Circle;
        }

        public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint)
        {
            var circle = entity as Circle;
            if (circle == null)
                return null;

            double newRadius = (throughPoint - circle.Center).Length;
            return newRadius > double.Epsilon
                ? new Circle(circle.Center, newRadius) { Thickness = circle.Thickness }
                : null;
        }

        public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint)
        {
            var circle = entity as Circle;
            if (circle == null)
                return null;

            double sideRadius = (sidePoint - circle.Center).Length;
            double newRadius = sideRadius >= circle.Radius
                ? circle.Radius + Math.Abs(distance)
                : circle.Radius - Math.Abs(distance);

            return newRadius > double.Epsilon
                ? new Circle(circle.Center, newRadius) { Thickness = circle.Thickness }
                : null;
        }
    }
}
