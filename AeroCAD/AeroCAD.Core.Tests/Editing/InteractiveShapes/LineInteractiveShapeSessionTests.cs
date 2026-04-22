using System.Windows;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class LineInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsLineState()
        {
            var session = new LineInteractiveShapeSession();
            session.Begin(new Point(0, 0));
            session.AddVertex(new Point(1, 1));
            session.AddSegment(new Line(new Point(0, 0), new Point(1, 1)));

            session.Reset();

            Assert.False(session.Drawing);
            Assert.Empty(session.Vertices);
            Assert.Empty(session.CreatedSegments);
        }

        [Fact]
        public void UndoLast_OnSecondPoint_RequestsDocumentUndo()
        {
            var session = new LineInteractiveShapeSession();
            session.Begin(new Point(0, 0));
            session.AddVertex(new Point(1, 1));
            session.AddSegment(new Line(new Point(0, 0), new Point(1, 1)));

            var ok = session.UndoLast(out var undoDocumentEntity);

            Assert.True(ok);
            Assert.True(undoDocumentEntity);
            Assert.Single(session.Vertices);
        }

        [Fact]
        public void Close_AppendsFirstPointWhenNeeded()
        {
            var session = new LineInteractiveShapeSession();
            session.Begin(new Point(0, 0));
            session.AddVertex(new Point(10, 0));
            session.AddSegment(new Line(new Point(0, 0), new Point(10, 0)));
            session.AddVertex(new Point(10, 10));
            session.AddSegment(new Line(new Point(10, 0), new Point(10, 10)));

            session.Close();

            Assert.Equal(new Point(0, 0), session.Vertices[session.Vertices.Count - 1]);
        }
    }
}
