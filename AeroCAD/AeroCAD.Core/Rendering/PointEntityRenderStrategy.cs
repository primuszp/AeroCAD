using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Rendering
{
    public sealed class PointEntityRenderStrategy : IEntityRenderStrategy, ISystemVariableConsumer
    {
        private ISystemVariableService systemVariables;

        public void SetSystemVariableService(ISystemVariableService systemVariables) => this.systemVariables = systemVariables;

        public bool CanHandle(Entity entity) => entity is PointEntity;

        public void Render(Entity entity, DrawingContext drawingContext, EntityRenderContext context)
        {
            var point = entity as PointEntity;
            if (point == null)
                return;

            var geometry = PointGeometryBuilder.Build(point.Location, GetPdMode(), ResolveDisplaySize(point.Scale));
            if (geometry == null || geometry.IsEmpty())
                return;

            if (context.HighlightGlowPen != null)
                drawingContext.DrawGeometry(null, context.HighlightGlowPen, geometry);
            if (context.HighlightPen != null)
                drawingContext.DrawGeometry(null, context.HighlightPen, geometry);

            drawingContext.DrawGeometry(null, context.Pen, geometry);
        }

        private int GetPdMode() => systemVariables?.Get(SystemVariableService.PdMode, 0) ?? 0;

        private double ResolveDisplaySize(double zoom)
        {
            double pdSize = systemVariables?.Get(SystemVariableService.PdSize, 0d) ?? 0d;
            double effectiveZoom = zoom > 1e-6 ? zoom : 1d;
            if (pdSize > 0d)
                return pdSize / effectiveZoom;
            if (pdSize < 0d)
                return (100d / effectiveZoom) * (-pdSize / 100d);
            return (100d / effectiveZoom) * 0.05d;
        }
    }
}
