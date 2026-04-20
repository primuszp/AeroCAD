using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class TrimExtendGeometryTests
    {
        [Fact]
        public void GetLineBoundaryIntersections_WithRestrictedTarget_ExcludesExtendedIntersection()
        {
            var target = new Line(new Point(0, 0), new Point(10, 0));
            var boundary = new Line(new Point(15, -5), new Point(15, 5));

            var restricted = TrimExtendGeometry.GetLineBoundaryIntersections(target, boundary);
            var unrestricted = TrimExtendGeometry.GetLineBoundaryIntersections(target, boundary, restrictTargetToSegment: false);

            Assert.Empty(restricted);
            var item = Assert.Single(unrestricted);
            Assert.Equal(new Point(15, 0), item.Point);
            Assert.True(item.Parameter > 1d);
        }

        [Fact]
        public void GetLineBoundaryIntersections_WithPolylineBoundary_DeduplicatesDuplicateHits()
        {
            var target = new Line(new Point(0, 0), new Point(10, 0));
            var boundary = new Polyline(new[]
            {
                new Point(5, -5),
                new Point(5, 5),
                new Point(5, 5),
                new Point(5, -5)
            });

            var intersections = TrimExtendGeometry.GetLineBoundaryIntersections(target, boundary);

            Assert.Single(intersections);
            Assert.Equal(new Point(5, 0), intersections[0].Point);
        }

        [Fact]
        public void GetCircularBoundaryIntersections_WithPolylineBoundary_ReturnsTwoPoints()
        {
            var center = new Point(0, 0);
            var boundary = new Polyline(new[]
            {
                new Point(-5, 0),
                new Point(5, 0)
            });

            var intersections = TrimExtendGeometry.GetCircularBoundaryIntersections(center, 5, boundary);

            Assert.Equal(2, intersections.Count);
            Assert.Contains(intersections, item => item.Point == new Point(-5, 0));
            Assert.Contains(intersections, item => item.Point == new Point(5, 0));
        }

        [Fact]
        public void GetCircularBoundaryIntersections_WithRectangleBoundary_ReturnsExpectedAngles()
        {
            var center = new Point(0, 0);
            var rect = new Rectangle(new Point(-5, -5), new Point(5, 5));

            var intersections = TrimExtendGeometry.GetCircularBoundaryIntersections(center, 5, rect).ToList();

            Assert.Equal(4, intersections.Count);
            Assert.Contains(intersections, item => item.Point == new Point(5, 0));
            Assert.Contains(intersections, item => item.Point == new Point(-5, 0));
            Assert.Contains(intersections, item => item.Point == new Point(0, 5));
            Assert.Contains(intersections, item => item.Point == new Point(0, -5));
        }
    }
}
