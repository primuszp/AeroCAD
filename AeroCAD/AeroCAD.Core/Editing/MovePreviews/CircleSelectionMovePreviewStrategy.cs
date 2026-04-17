using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public class CircleSelectionMovePreviewStrategy : ISelectionMovePreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Circle;
        }

        public GripPreview CreatePreview(Entity entity, Vector displacement)
        {
            var circle = entity as Circle;
            if (circle == null || circle.Radius <= 0d)
                return GripPreview.Empty;

            var movedGeometry = Circle.BuildGeometry(circle.Center + displacement, circle.Radius);
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
