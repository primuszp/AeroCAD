using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public class CircleGripPreviewStrategy : IGripPreviewStrategy
    {
        private const double HelperStrokeThickness = 1.5d;

        public bool CanHandle(Entity entity)
        {
            return entity is Circle;
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var circle = entity as Circle;
            if (circle == null)
                return GripPreview.Empty;

            if (gripIndex == 0)
            {
                Vector delta = newPosition - circle.Center;
                return CreatePreview(
                    entity,
                    new LineGeometry(circle.Center, newPosition),
                    Circle.BuildGeometry(circle.Center + delta, circle.Radius));
            }

            var originalGrip = circle.GetGripPoint(gripIndex);
            double newRadius = (newPosition - circle.Center).Length;
            return CreatePreview(
                entity,
                new LineGeometry(originalGrip, newPosition),
                Circle.BuildGeometry(circle.Center, newRadius));
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
