using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public class RectangleGripPreviewStrategy : IGripPreviewStrategy
    {
        private const double HelperStrokeThickness = 1.5d;

        public bool CanHandle(Entity entity) => entity is Rectangle;

        public GripPreview CreatePreview(Entity entity, int gripIndex, Point newPosition)
        {
            var rect = entity as Rectangle;
            if (rect == null) return GripPreview.Empty;

            Point topLeft = rect.TopLeft;
            Point bottomRight = rect.BottomRight;
            Point topRight = new Point(bottomRight.X, topLeft.Y);
            Point bottomLeft = new Point(topLeft.X, bottomRight.Y);
            Point center = new Point((topLeft.X + bottomRight.X) / 2, (topLeft.Y + bottomRight.Y) / 2);

            Point newTopLeft, newBottomRight;

            switch (gripIndex)
            {
                case 0: newTopLeft = newPosition; newBottomRight = bottomRight; break;
                case 1: newTopLeft = new Point(topLeft.X, newPosition.Y); newBottomRight = new Point(newPosition.X, bottomRight.Y); break;
                case 2: newTopLeft = topLeft; newBottomRight = newPosition; break;
                case 3: newTopLeft = new Point(newPosition.X, topLeft.Y); newBottomRight = new Point(bottomRight.X, newPosition.Y); break;
                case 4:
                    Vector delta = newPosition - center;
                    newTopLeft = topLeft + delta;
                    newBottomRight = bottomRight + delta;
                    break;
                default: return GripPreview.Empty;
            }

            var helperGeometry = new LineGeometry(rect.GetGripPoint(gripIndex), newPosition);
            var previewGeometry = new RectangleGeometry(new Rect(newTopLeft, newBottomRight));
            var entityColor = (entity.RenderHost as Layer)?.Color ?? Colors.White;

            return new GripPreview(new[]
            {
                GripPreviewStroke.CreateScreenConstant(helperGeometry, Colors.Orange, HelperStrokeThickness, DashStyles.Dash),
                GripPreviewStroke.CreateScreenConstant(previewGeometry, entityColor, entity.Thickness)
            });
        }
    }
}
