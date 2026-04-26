using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Rendering;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class XMarkerRenderStrategy : IEntityRenderStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is XMarkerEntity;
        }

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var marker = entity as XMarkerEntity;
            if (marker == null)
                return;

            double half = marker.Size / 2d;
            var a = new System.Windows.Point(marker.Center.X - half, marker.Center.Y - half);
            var b = new System.Windows.Point(marker.Center.X + half, marker.Center.Y + half);
            var c = new System.Windows.Point(marker.Center.X - half, marker.Center.Y + half);
            var d = new System.Windows.Point(marker.Center.X + half, marker.Center.Y - half);

            if (context.HighlightGlowPen != null)
            {
                drawingContext.DrawLine(context.HighlightGlowPen, a, b);
                drawingContext.DrawLine(context.HighlightGlowPen, c, d);
            }

            if (context.HighlightPen != null)
            {
                drawingContext.DrawLine(context.HighlightPen, a, b);
                drawingContext.DrawLine(context.HighlightPen, c, d);
            }

            drawingContext.DrawLine(context.Pen, a, b);
            drawingContext.DrawLine(context.Pen, c, d);
        }
    }
}
