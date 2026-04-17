using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public class LineSelectionMovePreviewStrategy : ISelectionMovePreviewStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Line;
        }

        public GripPreview CreatePreview(Entity entity, Vector displacement)
        {
            var line = entity as Line;
            if (line == null)
                return GripPreview.Empty;

            var movedGeometry = new LineGeometry(line.StartPoint + displacement, line.EndPoint + displacement);
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

