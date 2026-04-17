using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public class LineBoundsStrategy : IEntityBoundsStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Line;
        }

        public Rect GetBounds(Entity entity)
        {
            var line = entity as Line;
            if (line == null)
                return Rect.Empty;

            return new Rect(line.StartPoint, line.EndPoint);
        }
    }
}

