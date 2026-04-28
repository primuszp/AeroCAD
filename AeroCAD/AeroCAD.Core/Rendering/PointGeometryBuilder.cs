using System.Windows;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Rendering
{
    public static class PointGeometryBuilder
    {
        public static Geometry Build(Point center, int pdMode, double size)
        {
            if (pdMode == 1)
                return Geometry.Empty;

            double effectiveSize = size > 1e-6 ? size : 5d;
            double half = effectiveSize / 2d;
            var group = new GeometryGroup();
            int baseMode = pdMode & 31;

            if (baseMode == 0)
                group.Children.Add(new EllipseGeometry(center, 0.75d, 0.75d));
            else if (baseMode == 2)
                AddPlus(group, center, half);
            else if (baseMode == 3)
            {
                AddPlus(group, center, half);
                AddCross(group, center, half);
            }
            else if (baseMode == 4)
                group.Children.Add(new EllipseGeometry(center, half, half));
            else
                group.Children.Add(new EllipseGeometry(center, 0.75d, 0.75d));

            if ((pdMode & 32) == 32)
                group.Children.Add(new EllipseGeometry(center, half, half));
            if ((pdMode & 64) == 64)
                group.Children.Add(new RectangleGeometry(new Rect(new Point(center.X - half, center.Y - half), new Point(center.X + half, center.Y + half))));

            if (group.CanFreeze)
                group.Freeze();
            return group;
        }

        private static void AddPlus(GeometryGroup group, Point center, double half)
        {
            group.Children.Add(new LineGeometry(new Point(center.X - half, center.Y), new Point(center.X + half, center.Y)));
            group.Children.Add(new LineGeometry(new Point(center.X, center.Y - half), new Point(center.X, center.Y + half)));
        }

        private static void AddCross(GeometryGroup group, Point center, double half)
        {
            group.Children.Add(new LineGeometry(new Point(center.X - half, center.Y - half), new Point(center.X + half, center.Y + half)));
            group.Children.Add(new LineGeometry(new Point(center.X - half, center.Y + half), new Point(center.X + half, center.Y - half)));
        }
    }
}
