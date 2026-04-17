using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public class PolylineSelectionMovePreviewStrategy : ISelectionMovePreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Polyline;
        }

        public GripPreview CreatePreview(Entity entity, Vector displacement)
        {
            var polyline = entity as Polyline;
            if (polyline == null)
                return GripPreview.Empty;

            var movedPoints = polyline.Points.Select(point => point + displacement).ToList();
            var movedGeometry = Polyline.BuildGeometry(movedPoints);
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(movedGeometry, GetEntityColor(entity), entity.Thickness)
            });
        }

        private static Color GetEntityColor(Entity entity)
        {
            var layer = entity.RenderHost as Layer;
            return layer?.Color ?? Colors.White;
        }
    }
}

