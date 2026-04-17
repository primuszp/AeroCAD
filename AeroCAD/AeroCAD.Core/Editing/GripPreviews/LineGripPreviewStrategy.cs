using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public class LineGripPreviewStrategy : IGripPreviewStrategy
    {
        private const double HelperStrokeThickness = 1.5d;

        public bool CanHandle(Entity entity)
        {
            return entity is Line;
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var line = entity as Line;
            if (line == null)
                return GripPreview.Empty;

            if (gripIndex == 0 || gripIndex == 1)
            {
                Point originalGrip = line.GetGripPoint(gripIndex);
                Point fixedPoint = gripIndex == 0 ? line.EndPoint : line.StartPoint;
                return CreatePreview(entity, new LineGeometry(originalGrip, newPosition), new LineGeometry(fixedPoint, newPosition));
            }

            Point midpoint = line.GetGripPoint(gripIndex);
            Vector delta = newPosition - midpoint;
            Point translatedStart = line.StartPoint + delta;
            Point translatedEnd = line.EndPoint + delta;

            return CreatePreview(entity, new LineGeometry(midpoint, newPosition), new LineGeometry(translatedStart, translatedEnd));
        }

        private static GripPreview CreatePreview(Entity entity, Geometry helperGeometry, Geometry entityGeometry)
        {
            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(helperGeometry, Colors.Orange, HelperStrokeThickness, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(entityGeometry, GetEntityColor(entity), entity.Thickness)
            });
        }

        private static Color GetEntityColor(Entity entity)
        {
            var layer = entity.RenderHost as Layer;
            return layer?.Color ?? Colors.White;
        }
    }
}

