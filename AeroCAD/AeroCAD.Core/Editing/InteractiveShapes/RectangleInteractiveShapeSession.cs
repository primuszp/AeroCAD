using System.Windows;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.InteractiveShapes
{
    public sealed class RectangleInteractiveShapeSession : IInteractiveShapeSession
    {
        public bool HasFirstCorner { get; private set; }
        public Point FirstCorner { get; private set; }

        public void Reset()
        {
            HasFirstCorner = false;
            FirstCorner = default(Point);
        }

        public void Begin(Point firstCorner)
        {
            HasFirstCorner = true;
            FirstCorner = firstCorner;
        }

        public Rectangle BuildRectangle(Point oppositeCorner)
        {
            return HasFirstCorner ? new Rectangle(FirstCorner, oppositeCorner) : null;
        }

        public Polyline BuildPreview(Point rawPoint)
        {
            if (!HasFirstCorner)
                return null;

            var rect = new Rectangle(FirstCorner, rawPoint);
            var points = new List<Point>
            {
                new Point(rect.TopLeft.X, rect.TopLeft.Y),
                new Point(rect.BottomRight.X, rect.TopLeft.Y),
                new Point(rect.BottomRight.X, rect.BottomRight.Y),
                new Point(rect.TopLeft.X, rect.BottomRight.Y),
                new Point(rect.TopLeft.X, rect.TopLeft.Y)
            };

            return new Polyline(points);
        }
    }
}
