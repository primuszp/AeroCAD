using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Spatial
{
    public class PolylineBoundsStrategy : IEntityBoundsStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Polyline;
        }

        public Rect GetBounds(Entity entity)
        {
            var polyline = entity as Polyline;
            if (polyline == null || polyline.Points.Count == 0)
                return Rect.Empty;

            double minX = polyline.Points.Min(point => point.X);
            double minY = polyline.Points.Min(point => point.Y);
            double maxX = polyline.Points.Max(point => point.X);
            double maxY = polyline.Points.Max(point => point.Y);
            return new Rect(new Point(minX, minY), new Point(maxX, maxY));
        }
    }
}

