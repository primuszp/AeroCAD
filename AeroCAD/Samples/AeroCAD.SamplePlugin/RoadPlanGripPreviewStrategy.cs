using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class RoadPlanGripPreviewStrategy : IGripPreviewStrategy
    {
        public bool CanHandle(Entity entity) => entity is RoadPlanEntity;

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var roadPlan = entity as RoadPlanEntity;
            return roadPlan?.CreateGripPreview(gripIndex, newPosition) ?? GripPreview.Empty;
        }
    }
}
