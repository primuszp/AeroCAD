using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class CircleTrimExtendStrategyTests
    {
        [Fact]
        public void CreateTrimmed_WithRectangleBoundary_ReturnsTrimmedArc()
        {
            var strategy = new CircleTrimExtendStrategy();
            var target = new Circle(new Point(0, 0), 10);
            var boundaries = new List<Entity>
            {
                new Rectangle(new Point(-12, -4), new Point(12, 4))
            };

            var result = strategy.CreateTrimmed(boundaries, target, new Point(10, 0));

            var arc = Assert.IsType<Arc>(Assert.Single(result));
            Assert.Equal(10, arc.Radius, 6);
            Assert.True(arc.SweepAngle > 0);
            Assert.True(System.Math.Abs(arc.SweepAngle) > 0);
        }
    }
}
