using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class LineEntityRenderStrategy : IEntityRenderStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Line;
        }

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var line = entity as Line;
            if (line == null)
                return;

            var geometry = new LineGeometry(line.StartPoint, line.EndPoint);
            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);

            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);

            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }
    }
}

