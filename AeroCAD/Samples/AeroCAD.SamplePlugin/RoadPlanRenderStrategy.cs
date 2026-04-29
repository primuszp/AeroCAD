using System.Windows.Media;
using Primusz.AeroCAD.Core.Rendering;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanRenderStrategy : EntityRenderStrategy<RoadPlanEntity>
    {
        protected override void Render(RoadPlanEntity roadPlan, DrawingContext drawingContext, EntityRenderContext context)
        {
            var geometry = RoadPlanGeometryBuilder.BuildGeometry(roadPlan.Vertices);
            var tangentGeometry = RoadPlanGeometryBuilder.BuildTangentGeometry(roadPlan.ControlSegments);
            var tangentPen = CreateFrozenPen(Colors.White, 1.0d / (roadPlan.Scale > 1e-6 ? roadPlan.Scale : 1.0d));

            drawingContext.DrawGeometry(null, tangentPen, tangentGeometry);

            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);
            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);
            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }

        private static Pen CreateFrozenPen(Color color, double thickness)
        {
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
                brush.Freeze();

            var pen = new Pen(brush, thickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };
            if (pen.CanFreeze)
                pen.Freeze();
            return pen;
        }
    }
}
