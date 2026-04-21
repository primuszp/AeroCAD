using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.GeometryMath;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Geometry
{
    public class RegularPolygonGeometryTests
    {
        [Fact]
        public void BuildClosedPolygon_InscribedSquare_ReturnsClosedFourPointPolygon()
        {
            var points = RegularPolygonGeometry.BuildClosedPolygon(new Point(0, 0), 4, 10, 0, true);

            Assert.Equal(5, points.Count);
            Assert.Equal(points[0], points[points.Count - 1]);
            Assert.Contains(points, p => System.Math.Abs(p.X - 10d) < 1e-6 && System.Math.Abs(p.Y - 0d) < 1e-6);
            Assert.Contains(points, p => System.Math.Abs(p.X - 0d) < 1e-6 && System.Math.Abs(p.Y - 10d) < 1e-6);
        }

        [Fact]
        public void BuildClosedPolygon_CircumscribedHexagon_ReturnsClosedPolygon()
        {
            var points = RegularPolygonGeometry.BuildClosedPolygon(new Point(0, 0), 6, 10, 0, false);

            Assert.Equal(7, points.Count);
            Assert.Equal(points[0], points[points.Count - 1]);
            Assert.All(points.Take(6), p => Assert.True(System.Math.Abs(p.X) > 0 || System.Math.Abs(p.Y) > 0));
        }

        [Fact]
        public void BuildSidePolygon_ReturnsClosedPolygon()
        {
            Point center;
            double rotation;
            var points = RegularPolygonGeometry.BuildSidePolygon(
                new Point(-10, 0),
                new Point(10, 0),
                4,
                out center,
                out rotation);

            Assert.Equal(5, points.Length);
            Assert.Equal(points[0], points[points.Length - 1]);
            Assert.True(center.Y > 0);
            Assert.True(rotation > -System.Math.PI);
        }

        [Fact]
        public void BuildSidePolygon_PreservesInputEdge()
        {
            Point center;
            double rotation;
            var first = new Point(-10, 0);
            var second = new Point(10, 0);
            var points = RegularPolygonGeometry.BuildSidePolygon(first, second, 4, out center, out rotation);

            Assert.Contains(points, p => System.Math.Abs(p.X - first.X) < 1e-6 && System.Math.Abs(p.Y - first.Y) < 1e-6);
            Assert.Contains(points, p => System.Math.Abs(p.X - second.X) < 1e-6 && System.Math.Abs(p.Y - second.Y) < 1e-6);

            bool hasAdjacentEdge = false;
            for (int i = 0; i < points.Length - 1; i++)
            {
                bool firstMatch = System.Math.Abs(points[i].X - first.X) < 1e-6 && System.Math.Abs(points[i].Y - first.Y) < 1e-6;
                bool secondMatch = System.Math.Abs(points[i + 1].X - second.X) < 1e-6 && System.Math.Abs(points[i + 1].Y - second.Y) < 1e-6;
                bool reverseFirstMatch = System.Math.Abs(points[i].X - second.X) < 1e-6 && System.Math.Abs(points[i].Y - second.Y) < 1e-6;
                bool reverseSecondMatch = System.Math.Abs(points[i + 1].X - first.X) < 1e-6 && System.Math.Abs(points[i + 1].Y - first.Y) < 1e-6;
                if ((firstMatch && secondMatch) || (reverseFirstMatch && reverseSecondMatch))
                {
                    hasAdjacentEdge = true;
                    break;
                }
            }

            Assert.True(hasAdjacentEdge);
        }
    }
}
