using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Selection
{
    public interface ISelectionManager
    {
        IReadOnlyList<Entity> SelectedEntities { get; }

        event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        void Select(Entity entity);
        void SelectRange(IEnumerable<Entity> entities);
        void Deselect(Entity entity);
        void ClearSelection();
        bool IsSelected(Entity entity);
    }
}

