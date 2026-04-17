using System;
using System.Collections.Generic;
using System.Linq;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Selection
{
    public class SelectionManager : ISelectionManager
    {
        private readonly List<Entity> selected = new List<Entity>();

        public IReadOnlyList<Entity> SelectedEntities => selected.AsReadOnly();

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public void Select(Entity entity)
        {
            if (entity == null || selected.Contains(entity)) return;

            entity.Select();
            selected.Add(entity);
            RaiseSelectionChanged(new[] { entity }, Array.Empty<Entity>());
        }

        public void SelectRange(IEnumerable<Entity> entities)
        {
            var toAdd = entities.Where(e => e != null && !selected.Contains(e)).ToList();
            if (toAdd.Count == 0) return;

            foreach (var e in toAdd)
            {
                e.Select();
                selected.Add(e);
            }
            RaiseSelectionChanged(toAdd, Array.Empty<Entity>());
        }

        public void Deselect(Entity entity)
        {
            if (entity == null || !selected.Remove(entity)) return;

            entity.Unselect();
            RaiseSelectionChanged(Array.Empty<Entity>(), new[] { entity });
        }

        public void ClearSelection()
        {
            if (selected.Count == 0) return;

            var removed = selected.ToList();
            foreach (var e in removed)
                e.Unselect();
            selected.Clear();

            RaiseSelectionChanged(Array.Empty<Entity>(), removed);
        }

        public bool IsSelected(Entity entity) => entity != null && selected.Contains(entity);

        private void RaiseSelectionChanged(IReadOnlyList<Entity> added, IReadOnlyList<Entity> removed)
        {
            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(added, removed));
        }
    }
}

