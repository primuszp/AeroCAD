using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Primusz.Cadves.Core.Helpers;

namespace Primusz.Cadves.Core.Drawing.Layers
{
    public class RubberObject : FrameworkElement
    {
        #region Members

        private Point start, end;
        private RubberState currentState = RubberState.Idle;
        private RubberStyle currentStyle = RubberStyle.Line;
        private readonly Pen pen;

        #endregion

        #region Properties

        public bool Repetition { get; set; }

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

        public RubberObject(Viewport viewport)
        {
            viewport.Children.Add(this);
            Panel.SetZIndex(this, 1000);
            pen = new Pen(Brushes.White, 1.5);
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

            if (RubberState.Rubber != CurrentState) return;

            Brush brush = null;

            if (CurrentStyle == RubberStyle.Select)
            {
                if (end.X > start.X)
                    brush = new SolidColorBrush(Colors.DarkBlue) { Opacity = 0.5 };
                else
                    brush = new SolidColorBrush(Colors.ForestGreen) { Opacity = 0.5 };
            }

            Viewport viewport = VisualTreeHelpers.FindAncestor<Viewport>(this);
            pen.Thickness = 1.5 / viewport.Zoom;

            Geometry geometry = GetCurrentGeometry();
            context.DrawGeometry(brush, pen, geometry);
        }

        #endregion
    }
}
