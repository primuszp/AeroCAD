using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using WpCadCore.Controls;

namespace WpCadCore.Controls
{
    public class RubberLine : DrawingVisual
    {
        public enum RubberStyle { Line, Rectangle, Circle, Select }
        public enum RubberState { Idle, Start, Rubber }

        #region Private members

        private Pen pen;
        private Brush brush;
        private double thickness;
        private Point start, end;

        private RubberState currentState = RubberState.Idle;
        private RubberStyle currentStyle = RubberStyle.Line;

        #endregion

        #region Public properties

        public bool IsRepetition { get; set; }

        public RubberStyle CurrentStyle
        {
            get { return currentStyle; }
            set { currentStyle = value; }
        }

        public RubberState CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }

        public Point Start
        {
            get { return start; }
            set
            {
                start = value;
                Render();
            }
        }

        public Point End
        {
            get { return end; }
            set
            {
                end = value;
                Render();
            }
        }

        #endregion

        public RubberLine()
        {
            this.Opacity = 0.0d;
            this.thickness = 1.0d;
            this.IsRepetition = false;
            this.pen = new Pen(Brushes.White, thickness);
        }

        public void Render()
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                if (dc != null)
                {
                    switch (currentStyle)
                    {
                        case RubberStyle.Line:
                            {
                                Geometry geo = new LineGeometry(start, end);
                                dc.DrawGeometry(null, pen, geo);
                            }
                            break;
                        case RubberStyle.Select:
                            {
                                Geometry geo = new RectangleGeometry(new Rect(start, end));

                                if (end.X > start.X)
                                    this.brush = new SolidColorBrush(Colors.DarkBlue) { Opacity = 0.5 };
                                else
                                    this.brush = new SolidColorBrush(Colors.ForestGreen) { Opacity = 0.5 };

                                dc.DrawGeometry(brush, pen, geo);
                            }
                            break;
                        case RubberStyle.Rectangle:
                            {
                                Geometry geo = new RectangleGeometry(new Rect(start, end));
                                dc.DrawGeometry(null, pen, geo);
                            }
                            break;
                        case RubberStyle.Circle:
                            {
                                double radius = (start - end).Length;

                                GeometryGroup group = new GeometryGroup();
                                group.Children.Add(new LineGeometry(start, end));
                                group.Children.Add(new EllipseGeometry(start, radius, radius));

                                dc.DrawGeometry(brush, pen, group);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void SetStart(Point position)
        {
            currentState = RubberState.Start;
            {
                Start = position;
            }
            this.Opacity = 1;
        }

        public void SetStop(Point position)
        {
            switch (currentState)
            {
                case RubberState.Start:
                    {
                        currentState = RubberState.Idle;
                    }
                    break;
                case RubberState.Rubber:
                    {
                        End = position;
                        currentState = RubberState.Idle;
                    }
                    break;
            }
            this.Opacity = 0;
            if (IsRepetition == true) SetStart(position);
        }

        public void SetMove(Point position)
        {
            switch (currentState)
            {
                case RubberState.Idle:
                    break;
                case RubberState.Start:
                    {
                        End = position;
                        currentState = RubberState.Rubber;
                    }
                    break;
                case RubberState.Rubber:
                    {
                        End = position;
                    }
                    break;
            }
        }

        public void Cancel()
        {
            this.Opacity = 0;
            this.currentState = RubberState.Idle;
        }

        public void ScaleUpdate(double scale)
        {
            this.pen.Thickness = this.thickness * scale;
        }
    }
}
