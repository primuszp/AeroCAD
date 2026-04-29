using System.Windows;
using Primusz.AeroCAD.Core.Spatial;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanBoundsStrategy : EntityBoundsStrategy<RoadPlanEntity>
    {
        protected override Rect GetBounds(RoadPlanEntity roadPlan)
        {
            var axisBounds = RoadPlanGeometryBuilder.GetBounds(roadPlan.Vertices);
            var controlBounds = RoadPlanGeometryBuilder.GetControlBounds(roadPlan.ControlSegments);
            axisBounds.Union(controlBounds);
            return axisBounds;
        }
    }
}
