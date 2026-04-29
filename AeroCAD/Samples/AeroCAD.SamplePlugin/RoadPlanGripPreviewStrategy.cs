using System.Windows;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanGripPreviewStrategy : GripPreviewStrategy<RoadPlanEntity>
    {
        protected override GripPreview CreatePreview(RoadPlanEntity roadPlan, int gripIndex, Point newPosition)
        {
            return roadPlan.CreateGripPreview(gripIndex, newPosition);
        }
    }
}
