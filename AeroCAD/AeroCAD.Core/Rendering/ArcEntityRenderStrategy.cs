using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Rendering
{
    public class ArcEntityRenderStrategy : IEntityRenderStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Arc;
        }

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var arc = entity as Arc;
            if (arc == null || arc.Radius <= 0d || System.Math.Abs(arc.SweepAngle) <= double.Epsilon)
                return;

            var geometry = Arc.BuildGeometry(arc.Center, arc.Radius, arc.StartAngle, arc.SweepAngle);
            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);

            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);

            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }
    }
}
