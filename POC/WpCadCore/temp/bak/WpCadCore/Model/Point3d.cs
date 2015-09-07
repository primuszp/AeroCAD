using System.Windows;
using System.Windows.Media;

namespace WpCadCore.Model
{
    public class Point3d : IPoint
    {
        #region IPoint Members

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Color Color { get; set; }

        public Point3d()
            : base()
        {
            this.X = 0.0d;
            this.Y = 0.0d;
            this.Z = 0.0d;
            this.Color = Colors.White;
        }

        public Point3d(double x, double y)
            : base()
        {
            this.X = x;
            this.Y = y;
            this.Z = 0;
            this.Color = Colors.White;
        }

        public bool EqualCoordinates(IPoint other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        #endregion

        public static implicit operator Point(Point3d point)
        {
            return new Point(point.X, point.Y);
        }
    }
}
