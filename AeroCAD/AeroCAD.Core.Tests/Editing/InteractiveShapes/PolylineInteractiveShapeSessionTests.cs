using System.Windows;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class PolylineInteractiveShapeSessionTests
    {
        [Fact]
        public void BuildPreview_AppendsPreviewPoint()
        {
            var session = new PolylineInteractiveShapeSession();
            session.BeginPolyline(new Point(0, 0));

            var preview = session.BuildPreview(new Point(10, 0));

            Assert.NotNull(preview);
            Assert.Equal(2, preview.Points.Count);
        }

        [Fact]
        public void UndoLastPoint_ReturnsDocumentRemovalOnSecondPoint()
        {
            var session = new PolylineInteractiveShapeSession();
            session.BeginPolyline(new Point(0, 0));
            session.AddPoint(new Point(10, 0));

            var ok = session.UndoLastPoint(out var removeDocumentEntity);

            Assert.True(ok);
            Assert.True(removeDocumentEntity);
            Assert.Single(session.Points);
        }
    }
}
