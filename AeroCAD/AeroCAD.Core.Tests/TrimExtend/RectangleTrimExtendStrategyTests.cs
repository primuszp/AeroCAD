using System.Collections.Generic;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.TrimExtend;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.TrimExtend
{
    public class RectangleTrimExtendStrategyTests
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
    }
}
