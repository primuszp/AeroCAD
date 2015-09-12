using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Primusz.Cadves.Core.Drawing.Entities;
using Primusz.Cadves.Core.Drawing.Layers;

namespace Primusz.Cadves.Core.Tools
{
    public class SelectionTool : BaseTool, IMouseListener, IKeyboardListener
    {
        #region Members

        private Point start;
        private Point end;

        #endregion

        #region Constructors

        public SelectionTool()
            : base("SelectionTool")
        { }

        #endregion

        #region From IMouseListener interface

        public void MouseButtonDown(MouseEventArgs e)
        {
            if (!IsActive) return;

            var rbo = ToolService.Viewport.GetRubberObject();
            Point wpos = ToolService.Viewport.Position;

            rbo.CurrentStyle = RubberStyle.Select;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Kijelölés mutatóval
                bool successful = ToolService.Viewport.HitTest(wpos);

                if (rbo.CurrentState == RubberState.Rubber)
                {
                    rbo.SetStop(wpos);
                    end = rbo.End;

                    // Kijelölés dobozzal
                    ToolService.Viewport.HitTest(new Rect(start, end));
                }
                else if (successful == false)
                {
                    rbo.SetStart(wpos);
                    rbo.SetMove(wpos);

                    start = rbo.Start;
                    end = start;
                }
            }

            ToolService.GetService<Overlay>().Update();
            e.Handled = false;
        }

        public void MouseMove(MouseEventArgs e)
        {
            if (!IsActive) return;

            var rbo = ToolService.Viewport.GetRubberObject();
            rbo.SetMove(ToolService.Viewport.Position);

            e.Handled = true;
        }

        public void MouseButtonUp(MouseEventArgs e)
        {
            if (!IsActive) return;
            e.Handled = true;
        }

        public void MouseWheel(MouseWheelEventArgs e)
        {
            if (!IsActive) return;

            var rbo = ToolService.Viewport.GetRubberObject();
            rbo.InvalidateVisual();

            e.Handled = true;
        }

        #endregion

        #region From IKeyboardListener interface

        public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var layers = ToolService.Viewport.GetLayers();

                foreach (Layer layer in layers)
                {
                    foreach (Entity entity in layer.Entities)
                    {
                        if (entity.IsSelected)
                        {
                            entity.Unselect();
                        }
                    }
                }

                ToolService.GetService<Overlay>().Update();
                e.Handled = true;
            }
        }

        public void KeyUp(KeyEventArgs e)
        {
            if (!IsActive) return;
            e.Handled = true;
        }

        #endregion
    }
}
