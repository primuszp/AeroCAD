using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.MovePreviews
{
    public class RectangleSelectionMovePreviewStrategy : ISelectionMovePreviewStrategy
    {
        public bool CanHandle(Entity entity) => entity is Rectangle;

        public GripPreview CreatePreview(Entity entity, Vector displacement)
        {
            var rect = entity as Rectangle;
            if (rect == null) return GripPreview.Empty;

            var movedRect = new Rect(rect.TopLeft + displacement, rect.BottomRight + displacement);
            var geometry = new RectangleGeometry(movedRect);
            var color = (entity.RenderHost as Layer)?.Color ?? Colors.White;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(geometry, color, entity.Thickness)
            });
        }
    }
}
