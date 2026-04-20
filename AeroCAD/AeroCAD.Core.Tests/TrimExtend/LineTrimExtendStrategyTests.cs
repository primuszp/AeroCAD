using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class LineTrimExtendStrategyTests
    {
        [Fact]
        public void Extend_WithLineBoundary_ExtendsToOutsideIntersection()
        {
            var strategy = new LineTrimExtendStrategy();
            var target = new Line(new Point(0, 0), new Point(10, 0));
            var boundaries = new List<Entity> { new Line(new Point(15, -5), new Point(15, 5)) };

            var result = strategy.CreateExtended(boundaries, target, new Point(9, 0));

            var line = Assert.Single(result) as Line;
            Assert.NotNull(line);
            Assert.Equal(new Point(0, 0), line.StartPoint);
            Assert.Equal(new Point(15, 0), line.EndPoint);
        }

        [Fact]
        public void Trim_WithSingleIntersection_ReturnsRemainingSegment()
        {
            var strategy = new LineTrimExtendStrategy();
            var target = new Line(new Point(0, 0), new Point(10, 0));
            var boundaries = new List<Entity> { new Line(new Point(5, -5), new Point(5, 5)) };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(8, 0));

            var line = Assert.Single(result) as Line;
            Assert.NotNull(line);
            Assert.Equal(new Point(0, 0), line.StartPoint);
            Assert.Equal(new Point(5, 0), line.EndPoint);
        }

        [Fact]
        public void Extend_WithPolylineBoundary_ExtendsToOutsideIntersection()
        {
            var strategy = new LineTrimExtendStrategy();
            var target = new Line(new Point(0, 0), new Point(10, 0));
            var boundaries = new List<Entity>
            {
                new Polyline(new[]
                {
                    new Point(20, -5),
                    new Point(20, 5)
                })
            };

            var result = strategy.CreateExtended(boundaries, target, new Point(9, 0));

            var line = Assert.Single(result) as Line;
            Assert.NotNull(line);
            Assert.Equal(new Point(0, 0), line.StartPoint);
            Assert.Equal(new Point(20, 0), line.EndPoint);
        }
    }
}
