using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class PointBoundsStrategy : IEntityBoundsStrategy, ISystemVariableConsumer
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

        public Rect GetBounds(Entity entity)
        {
            var point = entity as PointEntity;
            if (point == null)
                return Rect.Empty;

            double half = PointDisplaySettings.ResolveDisplaySize(systemVariables, point.Scale) / 2d;
            return new Rect(
                point.Location.X - half,
                point.Location.Y - half,
                half * 2d,
                half * 2d);
        }
    }
}
