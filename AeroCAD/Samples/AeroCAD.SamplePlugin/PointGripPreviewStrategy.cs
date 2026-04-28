using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class PointGripPreviewStrategy : IGripPreviewStrategy, ISystemVariableConsumer
    {
        private const double HelperStrokeThickness = 1.5d;
        private ISystemVariableService systemVariables;

        public void SetSystemVariableService(ISystemVariableService systemVariables)
        {
            this.systemVariables = systemVariables;
        }

        public bool CanHandle(Entity entity)
        {
            return entity is PointEntity;
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var point = entity as PointEntity;
            if (point == null)
                return GripPreview.Empty;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(
                    new LineGeometry(point.Location, newPosition),
                    Colors.Orange,
                    HelperStrokeThickness,
                    DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(
                    PointGeometryBuilder.Build(
                        newPosition,
                        PointDisplaySettings.GetPdMode(systemVariables),
                        PointDisplaySettings.ResolveDisplaySize(systemVariables, point.Scale)),
                    Colors.White,
                    point.Thickness)
            });
        }
    }
}
