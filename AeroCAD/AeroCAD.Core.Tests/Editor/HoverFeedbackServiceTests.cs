using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editor;
using Primusz.AeroCAD.Core.Snapping;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editor
{
    public class HoverFeedbackServiceTests
    {
        [Fact]
        public void CanUpdateSnap_InIdleWithoutGrips_ReturnsFalse()
        {
            var service = new HoverFeedbackService();

            Assert.False(service.CanUpdateSnap(EditorMode.Idle, false));
        }

        [Fact]
        public void CanUpdateSnap_WithSelectedGrips_ReturnsTrue()
        {
            var service = new HoverFeedbackService();

            Assert.True(service.CanUpdateSnap(EditorMode.Idle, true));
        }

        [Fact]
        public void ResolveStatusPoint_WithGripSnap_ReturnsExactGripPoint()
        {
            var service = new HoverFeedbackService();
            var line = new Line(new Point(0, 0), new Point(100, 100));
            var snap = new SnapResult(new Point(50, 50), SnapType.Endpoint, new Point(50, 50), line, 1);

            var result = service.ResolveStatusPoint(EditorMode.Idle, true, snap);

            Assert.Equal(new Point(100, 100), result);
        }

        [Fact]
        public void ResolveStatusPoint_WithoutActiveModeAndNoGrip_ReturnsNull()
        {
            var service = new HoverFeedbackService();
            var snap = new SnapResult(new Point(50, 50), SnapType.Nearest, new Point(25, 25));

            var result = service.ResolveStatusPoint(EditorMode.Idle, false, snap);

            Assert.Null(result);
        }

        [Fact]
        public void ResolveStatusPoint_InCommandInput_ReturnsSourcePoint()
        {
            var service = new HoverFeedbackService();
            var snap = new SnapResult(new Point(50, 50), SnapType.Nearest, new Point(25, 25));

            var result = service.ResolveStatusPoint(EditorMode.CommandInput, false, snap);

            Assert.Equal(new Point(25, 25), result);
        }
    }
}
