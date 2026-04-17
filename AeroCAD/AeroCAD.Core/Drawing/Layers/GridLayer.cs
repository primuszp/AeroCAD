using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class GridLayer : FrameworkElement, IViewportSpaceElement, IViewportHostedElement
    {
        private readonly Viewport viewport;
        private readonly IGridSettingsService gridSettingsService;

        public ViewportCoordinateSpace CoordinateSpace => ViewportCoordinateSpace.Screen;

        public GridLayer(Viewport viewport, IGridSettingsService gridSettingsService)
        {
            this.viewport = viewport;
            this.gridSettingsService = gridSettingsService;
            IsHitTestVisible = false;

            viewport.Children.Add(this);
            Panel.SetZIndex(this, 0);

            if (this.gridSettingsService != null)
                this.gridSettingsService.StateChanged += (s, e) => InvalidateVisual();
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            if (viewport == null || gridSettingsService == null || !gridSettingsService.IsEnabled)
                return;

            if (viewport.ActualWidth <= 0 || viewport.ActualHeight <= 0 || viewport.Zoom <= 0)
                return;

            DrawGridLines(
                context,
                gridSettingsService.MinorSpacingX,
                gridSettingsService.MajorLineEvery,
                true,
                gridSettingsService.MinimumScreenSpacing,
                gridSettingsService.MinorLineColor,
                gridSettingsService.MajorLineColor);

            DrawGridLines(
                context,
                gridSettingsService.MinorSpacingY,
                gridSettingsService.MajorLineEvery,
                false,
                gridSettingsService.MinimumScreenSpacing,
                gridSettingsService.MinorLineColor,
                gridSettingsService.MajorLineColor);
        }

        public void UpdateViewportBounds(Size viewportSize)
        {
            Width = viewportSize.Width;
            Height = viewportSize.Height;
        }

        private void DrawGridLines(
            DrawingContext context,
            double baseSpacing,
            int majorEvery,
            bool verticalLines,
            double minimumScreenSpacing,
            Color minorColor,
            Color majorColor)
        {
            if (baseSpacing <= 0)
                return;

            double adaptiveSpacing = GetAdaptiveSpacing(baseSpacing, minimumScreenSpacing);
            int stepMultiplier = Math.Max(1, (int)Math.Round(adaptiveSpacing / baseSpacing));

            Rect visibleWorld = GetVisibleWorldBounds();
            double minimum = verticalLines
                ? Math.Min(visibleWorld.Left, visibleWorld.Right)
                : Math.Min(visibleWorld.Top, visibleWorld.Bottom);
            double maximum = verticalLines
                ? Math.Max(visibleWorld.Left, visibleWorld.Right)
                : Math.Max(visibleWorld.Top, visibleWorld.Bottom);

            long startIndex = (long)Math.Floor(minimum / adaptiveSpacing) - 1;
            long endIndex = (long)Math.Ceiling(maximum / adaptiveSpacing) + 1;

            var minorPen = CreatePen(minorColor);
            var majorPen = CreatePen(majorColor);

            for (long index = startIndex; index <= endIndex; index++)
            {
                double coordinate = index * adaptiveSpacing;
                bool isMajor = majorEvery > 0 && (index * stepMultiplier) % majorEvery == 0;
                var pen = isMajor ? majorPen : minorPen;

                Point worldStart;
                Point worldEnd;
                if (verticalLines)
                {
                    worldStart = new Point(coordinate, visibleWorld.Bottom);
                    worldEnd = new Point(coordinate, visibleWorld.Top);
                }
                else
                {
                    worldStart = new Point(visibleWorld.Left, coordinate);
                    worldEnd = new Point(visibleWorld.Right, coordinate);
                }

                context.DrawLine(pen, viewport.Project(worldStart), viewport.Project(worldEnd));
            }
        }

        private Rect GetVisibleWorldBounds()
        {
            Point topLeft = viewport.Unproject(new Point(0, 0));
            Point topRight = viewport.Unproject(new Point(viewport.ActualWidth, 0));
            Point bottomLeft = viewport.Unproject(new Point(0, viewport.ActualHeight));
            Point bottomRight = viewport.Unproject(new Point(viewport.ActualWidth, viewport.ActualHeight));

            double left = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
            double right = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
            double bottom = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
            double top = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

            return new Rect(new Point(left, bottom), new Point(right, top));
        }

        private double GetAdaptiveSpacing(double baseSpacing, double minimumScreenSpacing)
        {
            double spacing = baseSpacing;
            double screenSpacing = spacing * viewport.Zoom;

            while (screenSpacing < minimumScreenSpacing)
            {
                spacing *= 2.0d;
                screenSpacing = spacing * viewport.Zoom;
            }

            while (screenSpacing >= minimumScreenSpacing * 4.0d && spacing > baseSpacing)
            {
                spacing /= 2.0d;
                screenSpacing = spacing * viewport.Zoom;
            }

            return spacing;
        }

        private static Pen CreatePen(Color color)
        {
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
                brush.Freeze();

            var pen = new Pen(brush, 1.0d);
            if (pen.CanFreeze)
                pen.Freeze();

            return pen;
        }
    }
}

