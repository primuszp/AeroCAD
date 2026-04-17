using System.Windows;
using System.Windows.Input;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Tools
{
    public class PolylineTool : InteractiveCommandToolBase, IMouseListener, IKeyboardListener, ILayerBoundTool
    {
        private readonly PolylineCommandController controller;

        public Layer ActiveLayer { get; set; }

        public PolylineTool() : base("PolylineTool")
        {
            controller = new PolylineCommandController(() => ActiveLayer ?? GetFirstLayer());
        }

        public void MouseButtonDown(MouseEventArgs e)
        {
            if (!IsActive || IsSuspended || e.LeftButton != MouseButtonState.Pressed)
                return;

            ApplyResult(controller.TrySubmitViewportPoint(this, ToolService.Viewport.Position));
            e.Handled = true;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive || IsSuspended)
                return;

            controller.OnPointerMove(this, ToolService.Viewport.Position);
            e.Handled = true;
        }

        public void MouseButtonUp(MouseEventArgs e) { }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (!IsActive || IsSuspended)
                return;

            ToolService.Viewport.GetRubberObject().InvalidateVisual();
            e.Handled = true;
        }

        public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ApplyResult(controller.TryCancel(this));
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                ApplyResult(controller.TryComplete(this));
                e.Handled = true;
            }
        }

        public void KeyUp(KeyEventArgs e) { }

        public override bool Activate()
        {
            var activated = base.Activate();
            if (activated)
            {
                BeginInteractiveSession(controller.CommandName, controller.InitialStep, controller.EditorMode);
                controller.OnActivated(this);
            }

            return activated;
        }

        public override bool TrySubmitToken(Primusz.AeroCAD.Core.Editor.CommandInputToken token)
        {
            return ApplyResult(controller.TrySubmitToken(this, token));
        }

        public override bool TrySubmitPoint(Point point)
        {
            return ApplyResult(controller.TrySubmitToken(this, Primusz.AeroCAD.Core.Editor.CommandInputToken.Point(FormatPoint(point), point)));
        }

        public override bool TryComplete()
        {
            return ApplyResult(controller.TryComplete(this));
        }

        public override bool TryCancel()
        {
            return ApplyResult(controller.TryCancel(this));
        }

        private Layer GetFirstLayer()
        {
            var editorState = ToolService.GetService<Primusz.AeroCAD.Core.Editor.IEditorStateService>();
            if (editorState?.ActiveLayer != null)
                return editorState.ActiveLayer;

            var document = ToolService.GetService<Primusz.AeroCAD.Core.Documents.ICadDocumentService>();
            return document?.Layers.Count > 0 ? document.Layers[0] : null;
        }
    }
}

