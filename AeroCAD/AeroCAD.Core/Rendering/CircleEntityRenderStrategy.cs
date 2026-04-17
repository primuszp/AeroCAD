using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class CircleEntityRenderStrategy : IEntityRenderStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Circle;
        }

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var circle = entity as Circle;
            if (circle == null || circle.Radius <= 0d)
                return;

            var geometry = Circle.BuildGeometry(circle.Center, circle.Radius);
            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);

            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);

            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }
    }
}
