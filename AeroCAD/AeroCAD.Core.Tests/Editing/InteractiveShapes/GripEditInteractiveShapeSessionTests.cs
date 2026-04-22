using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Handles;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class GripEditInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsGripState()
        {
            var session = new GripEditInteractiveShapeSession();
            session.Reset();

            Assert.False(session.HasGrip);
            Assert.Null(session.ActiveGrip);
        }

        [Fact]
        public void BeginDrag_StoresGripAndPreviewState()
        {
            var entity = new Line(new Point(0, 0), new Point(10, 0));
            var grip = new Grip(entity, 0, null);
            var session = new GripEditInteractiveShapeSession();

            session.BeginDrag(grip);

            Assert.True(session.HasGrip);
            Assert.Same(grip, session.ActiveGrip);
            Assert.NotNull(session.StateBeforeDrag);
            Assert.Equal(grip.Owner.GetGripPoint(grip.Index), session.PreviewPosition);
            Assert.Equal(session.PreviewPosition, session.DragBasePoint);
            Assert.True(session.IgnoreNextMouseUp);
        }
    }
}
