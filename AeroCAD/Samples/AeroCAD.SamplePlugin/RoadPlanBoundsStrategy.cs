using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanBoundsStrategy : IEntityBoundsStrategy
    {
        public bool CanHandle(Entity entity) => entity is RoadPlanEntity;

        public Rect GetBounds(Entity entity)
        {
            var roadPlan = entity as RoadPlanEntity;
            return roadPlan == null ? Rect.Empty : RoadPlanGeometryBuilder.GetBounds(roadPlan.Vertices);
        }
    }
}
