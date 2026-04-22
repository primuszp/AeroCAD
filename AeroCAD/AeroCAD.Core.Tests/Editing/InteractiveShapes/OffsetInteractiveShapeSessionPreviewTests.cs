using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editing.TransientPreviews;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class OffsetInteractiveShapeSessionPreviewTests
    {
        [Fact]
        public void BuildPreview_UsesTransientPreviewService()
        {
            var session = new OffsetInteractiveShapeSession();
            var preview = session.BuildPreview(new TestTransientPreviewService(), new Line(new Point(0, 0), new Point(1, 1)), Colors.White);

            Assert.True(preview.HasContent);
        }

        private sealed class TestTransientPreviewService : ITransientEntityPreviewService
        {
            public GripPreview CreatePreview(Entity entity, Color color)
            {
                return new GripPreview(new[]
                {
                    GripPreviewStroke.CreateScreenConstant(new LineGeometry(new Point(0, 0), new Point(1, 1)), color, 1.0d)
                });
            }
        }
    }
}
