using System.Linq;
using System.Windows;
using System;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Editing.InteractiveShapes;
using Primusz.AeroCAD.Core.Selection;
using Xunit;

namespace Primusz.AeroCAD.Core.Tests.Editing.InteractiveShapes
{
    public class MoveCopyInteractiveShapeSessionTests
    {
        [Fact]
        public void Reset_ClearsSelectionAndBasePoint()
        {
            var session = new MoveCopyInteractiveShapeSession();
            session.BeginBasePoint(new Point(1, 2));
            session.Reset();

            Assert.False(session.HasBasePoint);
            Assert.Equal(new Point(), session.BasePoint);
            Assert.Empty(session.SelectedEntities);
        }

        [Fact]
        public void InitializeSelection_BuildsStateRecords()
        {
            var session = new MoveCopyInteractiveShapeSession();
            session.InitializeSelection(new TestSelectionManager(new[] { new Line(new Point(0, 0), new Point(1, 1)) }));

            Assert.Single(session.SelectedEntities);
            Assert.Single(session.StateRecords);
        }

        private sealed class TestSelectionManager : ISelectionManager
        {
            public TestSelectionManager(System.Collections.Generic.IEnumerable<Entity> selectedEntities)
            {
                SelectedEntities = selectedEntities.ToList().AsReadOnly();
            }

            public System.Collections.Generic.IReadOnlyList<Entity> SelectedEntities { get; }
            public event EventHandler<SelectionChangedEventArgs> SelectionChanged { add { } remove { } }
            public void ClearSelection() { }
            public void Deselect(Entity entity) { }
            public bool IsSelected(Entity entity) => SelectedEntities.Contains(entity);
            public void Select(Entity entity) { }
            public void SelectRange(System.Collections.Generic.IEnumerable<Entity> entities) { }
        }
    }
}
