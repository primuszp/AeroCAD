using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class PolylineEntityRenderStrategy : IEntityRenderStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Polyline;
        }

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var polyline = entity as Polyline;
            if (polyline == null)
                return;

            var geometry = Polyline.BuildGeometry(polyline.Points);
            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);

            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);

            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }
    }
}

