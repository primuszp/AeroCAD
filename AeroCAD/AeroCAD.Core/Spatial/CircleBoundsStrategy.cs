using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public class CircleBoundsStrategy : IEntityBoundsStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Circle;
        }

        public Rect GetBounds(Entity entity)
        {
            var circle = entity as Circle;
            if (circle == null || circle.Radius <= 0d)
                return Rect.Empty;

            return new Rect(
                circle.Center.X - circle.Radius,
                circle.Center.Y - circle.Radius,
                circle.Radius * 2d,
                circle.Radius * 2d);
        }
    }
}
