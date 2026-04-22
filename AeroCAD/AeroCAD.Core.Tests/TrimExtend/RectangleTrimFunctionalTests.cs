using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class RectangleTrimFunctionalTests
    {
        [Fact]
        public void Trim_WithCrossingLine_ReturnsPolyline()
        {
            var strategy = new RectangleTrimExtendStrategy();
            var target = new Rectangle(new Point(0, 0), new Point(10, 10));
            var boundaries = new List<Entity> { new Line(new Point(5, -5), new Point(5, 15)) };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(9, 5));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.True(polyline.Points.Count >= 4);
        }

        [Fact]
        public void Trim_WithCircleBoundary_ReturnsPolyline()
        {
            var strategy = new RectangleTrimExtendStrategy();
            var target = new Rectangle(new Point(0, 0), new Point(10, 10));
            var boundaries = new List<Entity> { new Circle(new Point(5, 5), 6) };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(9, 5));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.True(polyline.Points.Count >= 4);
            Assert.Contains(polyline.Points, p => System.Math.Abs(p.X - 10d) < 1e-6 && System.Math.Abs(p.Y - 1.683375209644602d) < 1e-6);
            Assert.Contains(polyline.Points, p => System.Math.Abs(p.X - 10d) < 1e-6 && System.Math.Abs(p.Y - 8.316624790355398d) < 1e-6);
        }

        [Fact]
        public void Trim_WithCrossingLine_ClickLeft_RemovesLeftHalf()
        {
            var strategy = new RectangleTrimExtendStrategy();
            var target = new Rectangle(new Point(0, 0), new Point(10, 10));
            var boundaries = new List<Entity> { new Line(new Point(5, -5), new Point(5, 15)) };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(2, 5));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.DoesNotContain(polyline.Points, p => p.X < 5.0d && p.Y > 0.0d && p.Y < 10.0d);
        }

        [Fact]
        public void Trim_WithCrossingLine_ClickRight_RemovesRightHalf()
        {
            var strategy = new RectangleTrimExtendStrategy();
            var target = new Rectangle(new Point(0, 0), new Point(10, 10));
            var boundaries = new List<Entity> { new Line(new Point(5, -5), new Point(5, 15)) };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(8, 5));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.DoesNotContain(polyline.Points, p => p.X > 5.0d && p.Y > 0.0d && p.Y < 10.0d);
        }
    }
}
