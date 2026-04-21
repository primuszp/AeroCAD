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
        public void PolylineTrim_WithLineBoundary_ClickRightOfBoundary_KeepsLeftPart()
        {
            // Click is to the RIGHT of the boundary (x=5) → the right part is trimmed → keep left
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
            Assert.Equal(2, polyline.Points.Count);
            Assert.Equal(new Point(0, 0), polyline.Points[0]);
            Assert.Equal(new Point(5, 0), polyline.Points[1]);
        }

        [Fact]
        public void PolylineTrim_WithLineBoundary_ClickLeftOfBoundary_KeepsRightPart()
        {
            // Click is to the LEFT of the boundary (x=5) → the left part is trimmed → keep right
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

            var result = strategy.CreateTrimmed(boundaries, target, new Point(2, 0));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.Equal(3, polyline.Points.Count);
            Assert.Equal(new Point(5, 0), polyline.Points[0]);
            Assert.Equal(new Point(10, 0), polyline.Points[1]);
            Assert.Equal(new Point(20, 0), polyline.Points[2]);
        }

        [Fact]
        public void PolylineTrim_WithPolylineBoundary_ClickRightOfBoundary_KeepsLeftPart()
        {
            // Click is to the RIGHT of the boundary (x=15) → the right part is trimmed → keep left
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

            var result = strategy.CreateTrimmed(boundaries, target, new Point(18, 0));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.Equal(3, polyline.Points.Count);
            Assert.Equal(new Point(0, 0), polyline.Points[0]);
            Assert.Equal(new Point(10, 0), polyline.Points[1]);
            Assert.Equal(new Point(15, 0), polyline.Points[2]);
        }

        [Fact]
        public void ClosedPolylineTrim_WithLineBoundary_ReturnsTwoSplitPaths()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(10, 10),
                new Point(0, 10),
                new Point(0, 0)
            });
            var boundaries = new List<Entity>
            {
                new Line(new Point(5, -5), new Point(5, 15))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(8, 5));

            Assert.Single(result);
            Assert.IsType<Polyline>(result[0]);
        }

        [Fact]
        public void ClosedPolylineTrim_WithLineBoundary_KeepsOppositeSide()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(10, 10),
                new Point(0, 10),
                new Point(0, 0)
            });
            var boundaries = new List<Entity>
            {
                new Line(new Point(5, -5), new Point(5, 15))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(8, 5));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.DoesNotContain(polyline.Points, p => p.X > 5.0d && p.Y > 0.0d && p.Y < 10.0d);
        }

        [Fact]
        public void ClosedPolylineTrim_WithLineBoundary_ClickLeft_RemovesLeftSide()
        {
            var strategy = new PolylineTrimExtendStrategy();
            var target = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(10, 10),
                new Point(0, 10),
                new Point(0, 0)
            });
            var boundaries = new List<Entity>
            {
                new Line(new Point(5, -5), new Point(5, 15))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(2, 5));

            var polyline = Assert.Single(result) as Polyline;
            Assert.NotNull(polyline);
            Assert.DoesNotContain(polyline.Points, p => p.X < 5.0d && p.Y > 0.0d && p.Y < 10.0d);
        }

    }
}
