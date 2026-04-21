using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.GripPreviews
{
    public class PolylineGripPreviewStrategyTests
    {
        [Fact]
        public void CreatePreview_ForMiddleGrip_DrawsOnlyAdjacentSegments()
        {
            var polyline = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(20, 0)
            });

            var strategy = new PolylineGripPreviewStrategy();

            var preview = strategy.CreatePreview(polyline, 1, new Point(10, 10));

            Assert.Equal(3, preview.Strokes.Count);
            Assert.Equal(2, preview.Strokes.Count(stroke => stroke.Color == System.Windows.Media.Colors.White));
            Assert.Equal(1, preview.Strokes.Count(stroke => stroke.Color == System.Windows.Media.Colors.Orange));
        }

        [Fact]
        public void CreatePreview_ForEndpointGrip_DrawsOnlySingleAffectedSegment()
        {
            var polyline = new Polyline(new[]
            {
                new Point(0, 0),
                new Point(10, 0),
                new Point(20, 0)
            });

            var strategy = new PolylineGripPreviewStrategy();

            var preview = strategy.CreatePreview(polyline, 0, new Point(0, 10));

            Assert.Equal(2, preview.Strokes.Count);
            Assert.Equal(1, preview.Strokes.Count(stroke => stroke.Color == System.Windows.Media.Colors.White));
            Assert.Equal(1, preview.Strokes.Count(stroke => stroke.Color == System.Windows.Media.Colors.Orange));
        }
    }
}
