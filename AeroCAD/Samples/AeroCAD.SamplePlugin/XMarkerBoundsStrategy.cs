using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class XMarkerBoundsStrategy : IEntityBoundsStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is XMarkerEntity;
        }

        public Rect GetBounds(Entity entity)
        {
            var marker = entity as XMarkerEntity;
            if (marker == null)
                return Rect.Empty;

            double half = marker.Size / 2d;
            return new Rect(
                marker.Center.X - half,
                marker.Center.Y - half,
                marker.Size,
                marker.Size);
        }
    }
}
