using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;

namespace Primusz.AeroCAD.SamplePlugin
{
    public sealed class XMarkerGripPreviewStrategy : IGripPreviewStrategy
    {
        private const double HelperStrokeThickness = 1.5d;

        public bool CanHandle(Entity entity)
        {
            return entity is XMarkerEntity;
        }

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var marker = entity as XMarkerEntity;
            if (marker == null)
                return GripPreview.Empty;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(
                    new LineGeometry(marker.Center, newPosition),
                    Colors.Orange,
                    HelperStrokeThickness,
                    DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(
                    BuildMarkerGeometry(newPosition, marker.Size),
                    Colors.White,
                    marker.Thickness)
            });
        }

        private static Geometry BuildMarkerGeometry(Point center, double size)
        {
            double half = size / 2d;
            var group = new GeometryGroup();
            group.Children.Add(new LineGeometry(
                new Point(center.X - half, center.Y - half),
                new Point(center.X + half, center.Y + half)));
            group.Children.Add(new LineGeometry(
                new Point(center.X - half, center.Y + half),
                new Point(center.X + half, center.Y - half)));
            return group;
        }
    }
}
