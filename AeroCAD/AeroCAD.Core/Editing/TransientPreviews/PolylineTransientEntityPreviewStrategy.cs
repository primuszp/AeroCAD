using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public class PolylineTransientEntityPreviewStrategy : ITransientEntityPreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Polyline;
        }

        public GripPreview CreatePreview(Entity entity, Color color)
        {
            var polyline = entity as Polyline;
            if (polyline == null || polyline.Points.Count < 2)
                return GripPreview.Empty;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(Polyline.BuildGeometry(polyline.Points), color, polyline.Thickness)
            });
        }
    }
}
