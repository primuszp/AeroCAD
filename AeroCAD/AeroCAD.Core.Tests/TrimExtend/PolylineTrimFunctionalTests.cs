using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class PolylineTrimFunctionalTests
    {
        [Fact]
        public void Trim_WithOneIntersectionAtEndpoint_DoesNotThrowAndCanTrim()
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
                new Line(new Point(0, -5), new Point(0, 5)),
                new Line(new Point(20, -5), new Point(20, 5))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(0, 0));

            Assert.NotNull(result);
        }

        [Fact]
        public void Extend_WithLineBoundary_ExtendsEndpoint()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0)
            });
            var boundaries = new List<Entity> { new Line(new Point(20, -5), new Point(20, 5)) };

            var result = strategy.CreateExtended(boundaries, target, new Point(9, 0));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.Equal(new Point(0, 0), polyline.Points[0]);
            Assert.Equal(new Point(20, 0), polyline.Points[1]);
        }

        [Fact]
        public void Extend_WithPolylineBoundary_ExtendsEndpoint()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0)
            });
            var boundaries = new List<Entity>
            {
                new Polyline(new[]
                {
                    new Point(20, -5),
                    new Point(20, 5)
                })
            };

            var result = strategy.CreateExtended(boundaries, target, new Point(9, 0));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.Equal(new Point(0, 0), polyline.Points[0]);
            Assert.Equal(new Point(20, 0), polyline.Points[1]);
        }

        [Fact]
        public void Extend_WithPolylineBoundary_UsingStartClick_ExtendsStartPoint()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0)
            });
            var boundaries = new List<Entity> { new Line(new Point(-10, -5), new Point(-10, 5)) };

            var result = strategy.CreateExtended(boundaries, target, new Point(1, 0));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.Equal(new Point(-10, 0), polyline.Points[0]);
            Assert.Equal(new Point(10, 0), polyline.Points[1]);
        }
    }
}
