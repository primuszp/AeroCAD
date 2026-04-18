using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.Core.Editing.TransientPreviews
{
    public class RectangleTransientEntityPreviewStrategy : ITransientEntityPreviewStrategy
    {
        public bool CanHandle(Entity entity) => entity is Rectangle;

        public GripPreview CreatePreview(Entity entity, Color color)
        {
            var rect = entity as Rectangle;
            if (rect == null) return GripPreview.Empty;

            var geometry = new RectangleGeometry(new Rect(rect.TopLeft, rect.BottomRight));
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(geometry, color, rect.Thickness)
            });
        }
    }
}
