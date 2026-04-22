using System.Linq;
using System.Windows;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using System.Windows.Media;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class PolygonInteractiveShapePreviewFactoryTests
    {
        [Fact]
        public void CreatePreview_ReturnsGrayCircleWhitePolygonAndOrangeGuide()
        {
            var factory = new PolygonInteractiveShapePreviewFactory();
            var context = new InteractiveShapePreviewContext(
                center: new Point(0, 0),
                cursor: new Point(10, 0),
                sides: 4,
                useInscribed: true,
                useEdgeMode: false,
                edgeStart: null);

            var preview = factory.CreatePreview(context);

            Assert.True(preview.HasContent);
            Assert.Equal(3, preview.Strokes.Count);
            Assert.Contains(preview.Strokes, stroke => stroke.Color == Colors.Orange);
            Assert.Contains(preview.Strokes, stroke => stroke.Color == Colors.LightGray);
            Assert.Contains(preview.Strokes, stroke => stroke.Color == Colors.White);
        }
    }
}
