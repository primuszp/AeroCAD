using System;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Documents
{
    public class EntityChangedEventArgs : EventArgs
    {
        public EntityChangedEventArgs(Entity entity, Layer layer)
        {
            Entity = entity;
            Layer = layer;
        }

        public Entity Entity { get; }

        public Layer Layer { get; }
    }
}

