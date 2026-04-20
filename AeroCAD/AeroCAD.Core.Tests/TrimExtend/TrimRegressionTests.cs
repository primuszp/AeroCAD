using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class TrimRegressionTests
    {
        [Fact]
        public void LineTrim_WithPolylineBoundary_ReturnsCorrectRemainingSegment()
        {
            var strategy = new LineTrimExtendStrategy();
            var target = new Line(new Point(0, 0), new Point(10, 0));
            var boundaries = new List<Entity>
            {
                new Polyline(new[]
                {
                    new Point(5, -5),
                    new Point(5, 5)
                })
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(8, 0));

            var line = Assert.Single(result) as Line;
            Assert.NotNull(line);
            Assert.Equal(new Point(0, 0), line.StartPoint);
            Assert.Equal(new Point(5, 0), line.EndPoint);
        }

        [Fact]
        public void PolylineTrim_WithLineBoundary_ReturnsCorrectRemainingSegment()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(20, 0)
            });
            var boundaries = new List<Entity>
            {
                new Line(new Point(5, -5), new Point(5, 5))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(15, 0));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.Equal(new Point(5, 0), polyline.Points[0]);
            Assert.Equal(new Point(10, 0), polyline.Points[1]);
        }

        [Fact]
        public void PolylineTrim_WithPolylineBoundary_ReturnsCorrectRemainingSegment()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(20, 0)
            });
            var boundaries = new List<Entity>
            {
                new Polyline(new[]
                {
                    new Point(15, -5),
                    new Point(15, 5)
                })
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(15, 0));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.Equal(new Point(0, 0), polyline.Points[0]);
            Assert.Equal(new Point(10, 0), polyline.Points[1]);
        }
    }
}
