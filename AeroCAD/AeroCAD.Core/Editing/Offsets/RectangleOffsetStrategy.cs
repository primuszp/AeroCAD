using System;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Editing.Offsets
{
    public class RectangleOffsetStrategy : IEntityOffsetStrategy
    {
        public bool CanHandle(Entity entity)
        {
            return entity is Rectangle;
        }

        public Entity CreateOffsetThroughPoint(Entity entity, Point throughPoint)
        {
            var rect = entity as Rectangle;
            if (rect == null)
                return null;

            Point center = new Point(
                (rect.TopLeft.X + rect.BottomRight.X) / 2,
                (rect.TopLeft.Y + rect.BottomRight.Y) / 2);

            // Signed distance: positive = outside, negative = inside
            double dx = throughPoint.X - center.X;
            double dy = throughPoint.Y - center.Y;

            // Project onto nearest axis to determine offset sign and magnitude
            double halfW = rect.Width / 2;
            double halfH = rect.Height / 2;

            // Normalize by half-extents to find dominant axis
            double nx = halfW > 0 ? Math.Abs(dx) / halfW : 0;
            double ny = halfH > 0 ? Math.Abs(dy) / halfH : 0;

            double signedDistance;
            if (nx >= ny)
                signedDistance = Math.Abs(dx) - halfW;
            else
                signedDistance = Math.Abs(dy) - halfH;

            return CreateOffsetRectangle(rect, signedDistance);
        }

        public Entity CreateOffsetByDistance(Entity entity, double distance, Point sidePoint)
        {
            var rect = entity as Rectangle;
            if (rect == null)
                return null;

            Point center = new Point(
                (rect.TopLeft.X + rect.BottomRight.X) / 2,
                (rect.TopLeft.Y + rect.BottomRight.Y) / 2);

            // Determine if side point is outside or inside the rectangle
            bool outside = sidePoint.X < rect.TopLeft.X || sidePoint.X > rect.BottomRight.X
                        || sidePoint.Y < rect.TopLeft.Y || sidePoint.Y > rect.BottomRight.Y;

            double signedDistance = outside ? Math.Abs(distance) : -Math.Abs(distance);
            return CreateOffsetRectangle(rect, signedDistance);
        }

        private static Rectangle CreateOffsetRectangle(Rectangle source, double signedDistance)
        {
            double newLeft   = source.TopLeft.X     - signedDistance;
            double newTop    = source.TopLeft.Y      - signedDistance;
            double newRight  = source.BottomRight.X  + signedDistance;
            double newBottom = source.BottomRight.Y  + signedDistance;

            // Collapsed rectangle — would invert; return null
            if (newRight <= newLeft || newBottom <= newTop)
                return null;

            return new Rectangle(new Point(newLeft, newTop), new Point(newRight, newBottom))
            {
                Thickness = source.Thickness
            };
        }
    }
}
