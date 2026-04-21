using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class ArcTrimExtendStrategyTests
    {
        [Fact]
        public void CreateTrimmed_WithTwoBoundaries_ReturnsTwoArcs()
        {
            var strategy = new ArcTrimExtendStrategy();
            var target = new Arc(new Point(0, 0), 10, 0, System.Math.PI);
            var boundaries = new List<Entity>
            {
                new Line(new Point(-5, -15), new Point(-5, 15)),
                new Line(new Point(5, -15), new Point(5, 15))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(0, 8));

            Assert.Equal(2, result.Count);
            var arcs = result.Cast<Arc>().ToList();
            Assert.True(arcs.All(arc => arc.Radius == 10));
            Assert.True(arcs[0].SweepAngle > 0);
            Assert.True(arcs[1].SweepAngle > 0);
            Assert.True(arcs[0].StartAngle < arcs[1].StartAngle);
        }

        [Fact]
        public void CreateTrimmed_WithTwoLineBoundaries_TrimmedMiddleSegment_ReturnsTwoArcs()
        {
            var strategy = new ArcTrimExtendStrategy();
            var target = new Arc(new Point(0, 0), 10, 0, System.Math.PI);
            var boundaries = new List<Entity>
            {
                new Line(new Point(-5, -15), new Point(-5, 15)),
                new Line(new Point(5, -15), new Point(5, 15))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(0, 8));

            var arcs = result.Cast<Arc>().OrderBy(arc => arc.StartAngle).ToList();
            Assert.Equal(2, arcs.Count);
            Assert.Equal(10, arcs[0].Radius, 6);
            Assert.Equal(10, arcs[1].Radius, 6);
            Assert.True(arcs[0].EndAngle < arcs[1].StartAngle);
        }
    }
}
