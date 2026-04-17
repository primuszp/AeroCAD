using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Markers;
using Primusz.AeroCAD.Core.Editing.GripPreviews;
using Primusz.AeroCAD.Core.Snapping;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class RubberObject : FrameworkElement, IViewportSpaceElement, IViewportHostedElement
    {
        #region Members

        private Point start, end;
        private RubberState currentState = RubberState.Idle;
        private RubberStyle currentStyle = RubberStyle.Line;
        private readonly Pen pen;
        private readonly Viewport viewport; // cached at construction
        private readonly IMarkerAppearanceService appearanceService;
        private GripPreview preview = GripPreview.Empty;
        private SnapResult snapPoint;

        #endregion

        #region Properties

        public bool Repetition { get; set; }

        public ViewportCoordinateSpace CoordinateSpace => ViewportCoordinateSpace.World;

        public GripPreview Preview
        {
            get { return preview; }
            set
            {
                preview = value ?? GripPreview.Empty;
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Current snap point to visualize (set by tools during MouseMove). Null = no snap active.
        /// </summary>
        public SnapResult SnapPoint
        {
            get { return snapPoint; }
            set
            {
                snapPoint = value;
                InvalidateVisual();
            }
        }

        public RubberStyle CurrentStyle
        {
            get { return currentStyle; }
            set
            {
                if (currentStyle == value) return;
                currentStyle = value;
                InvalidateVisual();
            }
        }

        public RubberState CurrentState
        {
            get { return currentState; }
            set
            {
                if (currentState == value) return;
                currentState = value;
                InvalidateVisual();
            }
        }

        public Point Start
        {
            get { return start; }
            set
            {
                start = value;
                InvalidateVisual();
            }
        }

        public Point End
        {
            get { return end; }
            set
            {
                end = value;
                InvalidateVisual();
            }
        }

        #endregion

        #region Constructors

        public RubberObject(Viewport viewport, IMarkerAppearanceService appearanceService)
        {
            this.viewport = viewport;
            this.appearanceService = appearanceService;
            IsHitTestVisible = false;
            viewport.Children.Add(this);
            Panel.SetZIndex(this, 1000);
            pen = new Pen(Brushes.White, 1.5);
            if (this.appearanceService != null)
                this.appearanceService.AppearanceChanged += (s, e) => InvalidateVisual();
        }

        #endregion

        #region Methods

        public void SetStart(Point position)
        {
            CurrentState = RubberState.Start;
            Start = position;
        }

        public void SetStop(Point position)
        {
            switch (currentState)
            {
                case RubberState.Start:
                    CurrentState = RubberState.Idle;
                    break;
                case RubberState.Rubber:
                    End = position;
                    CurrentState = RubberState.Idle;
                    break;
            }

            if (Repetition)
            {
                SetStart(position);
            }
        }

        public void SetMove(Point position)
        {
            switch (currentState)
            {
                case RubberState.Idle:
                    break;
                case RubberState.Start:
                    End = position;
                    CurrentState = RubberState.Rubber;
                    break;
                case RubberState.Rubber:
                    End = position;
                    break;
            }
        }

        public void Cancel()
        {
            CurrentState = RubberState.Idle;
        }

        #endregion

        #region Renders

        private Geometry GetCurrentGeometry()
        {
            Geometry geometry = null;

            switch (CurrentStyle)
            {
                case RubberStyle.Line:
                    geometry = new LineGeometry(start, end);
                    break;
                case RubberStyle.Select:
                case RubberStyle.Rectangle:
                    geometry = new RectangleGeometry(new Rect(start, end));
                    break;
                case RubberStyle.Circle:
                    double radius = (Start - End).Length;
                    GeometryGroup group = new GeometryGroup();
                    group.Children.Add(new LineGeometry(start, end));
                    group.Children.Add(new EllipseGeometry(start, radius, radius));
                    geometry = group;
                    break;
            }
            return geometry;
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            if (RubberState.Rubber == CurrentState)
            {
                Brush brush = null;

                if (CurrentStyle == RubberStyle.Select)
                {
                    if (end.X > start.X)
                        brush = new SolidColorBrush(Colors.DarkBlue) { Opacity = 0.5 };
                    else
                        brush = new SolidColorBrush(Colors.ForestGreen) { Opacity = 0.5 };
                }

                pen.Thickness = 1.5 / viewport.Zoom;

                Geometry geometry = GetCurrentGeometry();
                context.DrawGeometry(brush, pen, geometry);
            }

            // Snap marker is drawn regardless of rubber state (active during grip drag too)
            DrawPreview(context);
            DrawSnapMarker(context);
        }

        private void DrawPreview(DrawingContext context)
        {
            if (Preview == null || !Preview.HasContent) return;

            foreach (var stroke in Preview.Strokes.Where(item => item?.Geometry != null))
                context.DrawGeometry(null, stroke.CreatePen(viewport.Zoom), stroke.Geometry);
        }

        private void DrawSnapMarker(DrawingContext context)
        {
            if (SnapPoint == null) return;

            // Snap markers are drawn in world space (layer transform handles zoom/pan)
            double size = (appearanceService?.MarkerSize ?? 10.0d) / viewport.Zoom;
            double strokeThickness = (appearanceService?.MarkerStrokeThickness ?? 1.5d) / viewport.Zoom;
            var snapStrokeBrush = new SolidColorBrush(appearanceService?.SnapStrokeColor ?? Colors.Yellow);
            var snapHoverBrush = new SolidColorBrush(appearanceService?.SnapHoverColor ?? Colors.Orange) { Opacity = 0.35 };
            if (snapStrokeBrush.CanFreeze)
                snapStrokeBrush.Freeze();
            if (snapHoverBrush.CanFreeze)
                snapHoverBrush.Freeze();

            var snapPen = new Pen(snapStrokeBrush, strokeThickness);
            var pt = SnapPoint.Point;

            if (SnapPoint.Type == SnapType.Endpoint)
            {
                context.DrawRectangle(snapHoverBrush, snapPen,
                    new Rect(pt.X - size / 2, pt.Y - size / 2, size, size));
            }
            else if (SnapPoint.Type == SnapType.Midpoint)
            {
                var geo = new StreamGeometry();
                using (var sgc = geo.Open())
                {
                    sgc.BeginFigure(new Point(pt.X, pt.Y - size / 2), false, true);
                    sgc.LineTo(new Point(pt.X + size / 2, pt.Y + size / 2), true, false);
                    sgc.LineTo(new Point(pt.X - size / 2, pt.Y + size / 2), true, false);
                }
                context.DrawGeometry(snapHoverBrush, snapPen, geo);
            }
            else if (SnapPoint.Type == SnapType.Center)
            {
                double radius = size / 2;
                context.DrawEllipse(snapHoverBrush, snapPen, pt, radius, radius);
                context.DrawLine(snapPen, new Point(pt.X - radius, pt.Y), new Point(pt.X + radius, pt.Y));
                context.DrawLine(snapPen, new Point(pt.X, pt.Y - radius), new Point(pt.X, pt.Y + radius));
            }
            else if (SnapPoint.Type == SnapType.Quadrant)
            {
                var geo = new StreamGeometry();
                using (var sgc = geo.Open())
                {
                    sgc.BeginFigure(new Point(pt.X, pt.Y - size / 2), true, true);
                    sgc.LineTo(new Point(pt.X + size / 2, pt.Y), true, false);
                    sgc.LineTo(new Point(pt.X, pt.Y + size / 2), true, false);
                    sgc.LineTo(new Point(pt.X - size / 2, pt.Y), true, false);
                }
                context.DrawGeometry(snapHoverBrush, snapPen, geo);
            }
            else
            {
                context.DrawEllipse(snapHoverBrush, snapPen, pt, size / 2, size / 2);
            }
        }

        public void UpdateViewportBounds(Size viewportSize)
        {
            Width = viewportSize.Width;
            Height = viewportSize.Height;
        }

        public void ClearPreview()
        {
            Preview = GripPreview.Empty;
        }

        #endregion
    }
}

