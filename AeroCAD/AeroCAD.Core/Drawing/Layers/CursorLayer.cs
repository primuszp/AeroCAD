using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class CursorLayer : ViewportHostedScreenLayerBase
    {
        private Point mouseScreenPos;
        private readonly Pen crosshairPen;
        private readonly Pen pickboxPen;
        private readonly double pickboxSize = 10.0;

        public CursorLayer(Viewport viewport)
            : base(viewport, 2000)
        {
            SnapsToDevicePixels = true;

            crosshairPen = new Pen(Brushes.White, 1.0);
            pickboxPen = new Pen(Brushes.White, 1.0);

            viewport.MouseMove += Viewport_MouseMove;
            viewport.MouseEnter += Viewport_MouseEnter;
            viewport.MouseLeave += Viewport_MouseLeave;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            viewport.Cursor = Cursors.None;
        }

        private void Viewport_MouseEnter(object sender, MouseEventArgs e)
        {
            viewport.Cursor = Cursors.None;
            mouseScreenPos = e.GetPosition(viewport);
            InvalidateVisual();
        }

        private void Viewport_MouseLeave(object sender, MouseEventArgs e)
        {
            InvalidateVisual();
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            mouseScreenPos = e.GetPosition(viewport);
            InvalidateVisual();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!viewport.IsLoaded || !viewport.IsMouseOver)
                return;

            var currentPosition = Mouse.GetPosition(viewport);
            if (currentPosition != mouseScreenPos)
            {
                mouseScreenPos = currentPosition;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            if (!viewport.IsMouseOver)
                return;

            if (viewport.IsMouseCaptured && viewport.Cursor == Cursors.ScrollAll)
                return;

            var cursorType = viewport.ActiveCursorType;
            if (cursorType == CadCursorType.None)
                return;

            Point center = GetSnappedScreenPos();

            if (cursorType == CadCursorType.CrosshairOnly || cursorType == CadCursorType.CrosshairAndPickbox)
            {
                context.DrawLine(crosshairPen, new Point(0, center.Y), new Point(ActualWidth, center.Y));
                context.DrawLine(crosshairPen, new Point(center.X, 0), new Point(center.X, ActualHeight));
            }

            if (cursorType == CadCursorType.PickboxOnly || cursorType == CadCursorType.CrosshairAndPickbox)
            {
                double half = pickboxSize / 2.0;
                context.DrawRectangle(null, pickboxPen, new Rect(center.X - half, center.Y - half, pickboxSize, pickboxSize));
            }
        }

        private Point GetSnappedScreenPos()
        {
            var rubberObject = viewport.GetRubberObject();
            if (rubberObject?.SnapPoint != null)
                return viewport.Project(rubberObject.SnapPoint.Point);

            return mouseScreenPos;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(viewport.ActualWidth, viewport.ActualHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return new Size(viewport.ActualWidth, viewport.ActualHeight);
        }
    }
}

