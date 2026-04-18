using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public class RectangleBoundsStrategy : IEntityBoundsStrategy
    {
        public bool CanHandle(Entity entity) => entity is Rectangle;

        public Rect GetBounds(Entity entity)
        {
            var rect = entity as Rectangle;
            if (rect == null) return Rect.Empty;

            return new Rect(rect.TopLeft, rect.BottomRight);
        }
    }
}
