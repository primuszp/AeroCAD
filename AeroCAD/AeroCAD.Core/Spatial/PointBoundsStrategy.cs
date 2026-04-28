using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Spatial
{
    public sealed class PointBoundsStrategy : IEntityBoundsStrategy, ISystemVariableConsumer
    {
        private ISystemVariableService systemVariables;

        public void SetSystemVariableService(ISystemVariableService systemVariables) => this.systemVariables = systemVariables;

        public bool CanHandle(Entity entity) => entity is PointEntity;

        public Rect GetBounds(Entity entity)
        {
            var point = entity as PointEntity;
            if (point == null)
                return Rect.Empty;

            double half = ResolveDisplaySize(point.Scale) / 2d;
            return new Rect(point.Location.X - half, point.Location.Y - half, half * 2d, half * 2d);
        }

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
