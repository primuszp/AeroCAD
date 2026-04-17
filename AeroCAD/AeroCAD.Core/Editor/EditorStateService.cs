using System;
using Primusz.AeroCAD.Core.Drawing;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Editor
{
    public class EditorStateService : IEditorStateService
    {
        private readonly IViewport viewport;
        private EditorMode mode;
        private Layer activeLayer;

        public EditorStateService(IViewport viewport)
        {
            this.viewport = viewport;
            mode = EditorMode.Idle;
            ApplyCursor();
        }

        public EditorMode Mode => mode;

        public Layer ActiveLayer => activeLayer;

        public event EventHandler StateChanged;

        public void SetMode(EditorMode mode)
        {
            if (this.mode == mode)
                return;

            this.mode = mode;
            ApplyCursor();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetActiveLayer(Layer layer)
        {
            if (ReferenceEquals(activeLayer, layer))
                return;

            activeLayer = layer;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyCursor()
        {
            if (viewport == null)
                return;

            viewport.ActiveCursorType = GetCursorType(mode);
            viewport.InvalidateVisual();
        }

        private static CadCursorType GetCursorType(EditorMode mode)
        {
            switch (mode)
            {
                case EditorMode.SelectionWindow:
                    return CadCursorType.PickboxOnly;
                case EditorMode.CommandInput:
                case EditorMode.GripEditing:
                    return CadCursorType.CrosshairOnly;
                default:
                    return CadCursorType.CrosshairAndPickbox;
            }
        }
    }
}

