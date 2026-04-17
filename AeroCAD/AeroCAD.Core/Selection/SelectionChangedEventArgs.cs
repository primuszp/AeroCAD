using System;
using System.Collections.Generic;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.Core.Selection
{
    public class SelectionChangedEventArgs : EventArgs
    {
        public IReadOnlyList<Entity> Added { get; }
        public IReadOnlyList<Entity> Removed { get; }

        public SelectionChangedEventArgs(IReadOnlyList<Entity> added, IReadOnlyList<Entity> removed)
        {
            Added = added;
            Removed = removed;
        }
    }
}

