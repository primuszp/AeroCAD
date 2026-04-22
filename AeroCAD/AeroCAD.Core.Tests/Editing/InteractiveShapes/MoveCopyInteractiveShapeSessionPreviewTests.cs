using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Editing.MovePreviews;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class MoveCopyInteractiveShapeSessionPreviewTests
    {
        [Fact]
        public void BuildPreview_AppendsOrangeGuideLine()
        {
            var session = new MoveCopyInteractiveShapeSession();
            session.InitializeSelection(null);
            session.BeginBasePoint(new Point(0, 0));

            var preview = session.BuildPreview(new TestMovePreviewService(), new Point(10, 0));

            Assert.True(preview.HasContent);
            Assert.Contains(preview.Strokes, stroke => stroke.Color == Colors.Orange);
        }

        private sealed class TestMovePreviewService : ISelectionMovePreviewService
        {
            public GripPreview CreatePreview(IEnumerable<Entity> entities, Vector displacement)
            {
                return GripPreview.Empty;
            }
        }
    }
}
