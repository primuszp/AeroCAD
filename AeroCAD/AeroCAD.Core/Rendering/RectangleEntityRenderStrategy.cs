using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class RectangleEntityRenderStrategy : IEntityRenderStrategy
    {
        public bool CanHandle(Entity entity) => entity is Rectangle;

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var rect = entity as Rectangle;
            if (rect == null) return;

            var geometry = new RectangleGeometry(new Rect(rect.TopLeft, rect.BottomRight));

            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);

            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);

            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }
    }
}
