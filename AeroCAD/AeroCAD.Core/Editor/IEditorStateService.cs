using System;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Editor
{
    public interface IEditorStateService
    {
        EditorMode Mode { get; }

        Layer ActiveLayer { get; }

        event EventHandler StateChanged;

        void SetMode(EditorMode mode);

        void SetActiveLayer(Layer layer);
    }
}

