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
            if (gripIndex < 0 || gripIndex >= previewPoints.Count)
                return GripPreview.Empty;

            Point originalGrip = previewPoints[gripIndex];
            previewPoints[gripIndex] = newPosition;

            var strokes = new List<GripPreviewStroke>
            {
                GripPreviewStroke.CreateScreenConstant(new LineGeometry(originalGrip, newPosition), Colors.Orange, HelperStrokeThickness, DashStyles.Dash)
            };

            if (gripIndex > 0)
            {
                strokes.Add(GripPreviewStroke.CreateScreenConstant(
                    new LineGeometry(previewPoints[gripIndex - 1], previewPoints[gripIndex]),
                    GetEntityColor(entity),
                    entity.Thickness));
            }

            if (gripIndex < previewPoints.Count - 1)
            {
                strokes.Add(GripPreviewStroke.CreateScreenConstant(
                    new LineGeometry(previewPoints[gripIndex], previewPoints[gripIndex + 1]),
                    GetEntityColor(entity),
                    entity.Thickness));
            }

            return new GripPreview(strokes);
        }

        private static Color GetEntityColor(Entity entity)
        {
            var layer = entity.RenderHost as Layer;
            return layer?.Color ?? Colors.White;
        }
    }
}

