using System.Windows;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class RectangleInteractiveShapeSessionTests
    {
        [Fact]
        public void BuildRectangle_ReturnsRectangleFromStoredCorner()
        {
            var session = new RectangleInteractiveShapeSession();
            session.Begin(new Point(0, 0));

            var rect = session.BuildRectangle(new Point(10, 5));

            Assert.NotNull(rect);
            Assert.Equal(new Point(0, 0), rect.TopLeft);
            Assert.Equal(new Point(10, 5), rect.BottomRight);
        }

        [Fact]
        public void BuildPreview_ReturnsPolylinePreview()
        {
            var session = new RectangleInteractiveShapeSession();
            session.Begin(new Point(0, 0));

            var preview = session.BuildPreview(new Point(10, 5));

            Assert.NotNull(preview);
            Assert.True(preview.Points.Count >= 4);
        }
    }
}
