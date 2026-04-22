using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using System.Windows;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class TrimExtendInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsBoundaryState()
        {
            var session = new TrimExtendInteractiveShapeSession();
            session.AddBoundary(new Line(new Point(0, 0), new Point(1, 1)));
            session.SetTargetHighlight(new Line(new Point(1, 1), new Point(2, 2)));

            session.Reset();

            Assert.Empty(session.BoundaryEntities);
            Assert.Empty(session.HighlightedBoundaries);
            Assert.Null(session.HighlightedTargetEntity);
        }

        [Fact]
        public void AddBoundary_PreventsDuplicates()
        {
            var session = new TrimExtendInteractiveShapeSession();
            var line = new Line(new Point(0, 0), new Point(1, 1));

            Assert.True(session.AddBoundary(line));
            Assert.False(session.AddBoundary(line));
            Assert.Single(session.BoundaryEntities);
        }
    }
}
