using System;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Documents
{
    public class LayerChangedEventArgs : EventArgs
    {
        public LayerChangedEventArgs(Layer layer)
        {
            Layer = layer;
        }

        public Layer Layer { get; }
    }
}

