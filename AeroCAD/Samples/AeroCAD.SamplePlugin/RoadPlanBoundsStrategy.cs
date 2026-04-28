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
            if (roadPlan == null)
                return Rect.Empty;

            var axisBounds = RoadPlanGeometryBuilder.GetBounds(roadPlan.Vertices);
            var controlBounds = RoadPlanGeometryBuilder.GetControlBounds(roadPlan.ControlSegments);
            axisBounds.Union(controlBounds);
            return axisBounds;
        }
    }
}
