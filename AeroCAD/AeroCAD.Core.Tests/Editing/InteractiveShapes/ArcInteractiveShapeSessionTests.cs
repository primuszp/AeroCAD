using System.Windows;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class ArcInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsArcState()
        {
            var session = new ArcInteractiveShapeSession();
            session.BeginStart(new Point(1, 2));
            session.BeginSecond(new Point(3, 4));

            session.Reset();

            Assert.Equal(ArcInteractiveShapeSession.ArcPhase.WaitingForStart, session.Phase);
        }

        [Fact]
        public void BuildLinePreview_ReturnsPreviewInSecondPointPhase()
        {
            var session = new ArcInteractiveShapeSession();
            session.BeginStart(new Point(0, 0));

            var preview = session.BuildLinePreview(new Point(10, 0));

            Assert.True(preview.HasContent);
        }

        [Fact]
        public void BuildArcPreview_ReturnsPreviewInEndPhase()
        {
            var session = new ArcInteractiveShapeSession();
            session.BeginStart(new Point(0, 0));
            session.BeginSecond(new Point(10, 10));

            var preview = session.BuildArcPreview(new Point(20, 0));

            Assert.True(preview.HasContent);
        }
    }
}
