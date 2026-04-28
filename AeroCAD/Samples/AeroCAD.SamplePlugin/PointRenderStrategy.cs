using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Rendering;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class PointRenderStrategy : IEntityRenderStrategy, ISystemVariableConsumer
    {
        private ISystemVariableService systemVariables;

        public void SetSystemVariableService(ISystemVariableService systemVariables)
        {
            this.systemVariables = systemVariables;
        }

        public bool CanHandle(Entity entity)
        {
            return entity is PointEntity;
        }

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var point = entity as PointEntity;
            if (point == null)
                return;

            var geometry = PointGeometryBuilder.Build(
                point.Location,
                PointDisplaySettings.GetPdMode(systemVariables),
                PointDisplaySettings.ResolveDisplaySize(systemVariables, point.Scale));

            if (geometry == null || geometry.IsEmpty())
                return;

            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);

            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);

            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }
    }
}
