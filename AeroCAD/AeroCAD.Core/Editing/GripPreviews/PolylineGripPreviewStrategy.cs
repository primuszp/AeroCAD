using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public class PolylineGripPreviewStrategy : IGripPreviewStrategy
    {
        private const double HelperStrokeThickness = 1.5d;

        public bool CanHandle(Entity entity)
        {
            return entity is Polyline;
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var polyline = entity as Polyline;
            if (polyline == null)
                return GripPreview.Empty;

            var previewPoints = new List<Point>(polyline.Points);
            Point originalGrip = previewPoints[gripIndex];
            previewPoints[gripIndex] = newPosition;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(originalGrip, newPosition), Colors.Orange, HelperStrokeThickness, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(BuildGeometry(previewPoints), GetEntityColor(entity), entity.Thickness)
            });
        }

        private static Geometry BuildGeometry(IReadOnlyList<Point> sourcePoints)
        {
            return Polyline.BuildGeometry(sourcePoints);
        }

        private static Color GetEntityColor(Entity entity)
        {
            var layer = entity.RenderHost as Layer;
            return layer?.Color ?? Colors.White;
        }
    }
}

