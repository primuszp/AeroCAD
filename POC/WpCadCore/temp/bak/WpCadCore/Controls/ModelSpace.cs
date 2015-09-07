using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using WpCadCore.Model;

namespace WpCadCore.Controls
{
    class ModelSpace : Canvas, IModelSpace
    {
        #region Private members

        private TranslateTransform translate;
        private ScaleTransform scale;
        private Point startOffset;
        private Point startPoint;

        private ScreenLayer screen;

        #endregion

        #region Public properties

        public RubberLine RubberLine { get; private set; }

        public TransformGroup ViewTransform { get; set; }

        public TranslateTransform Translate
        {
            get { return translate; }
            set { translate = value; }
        }

        public ScaleTransform Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public Point WorldPosition { get; set; }

        public double Zoom { get; set; }

        #endregion

        public ModelSpace()
        {
            this.ClipToBounds = true;

            this.LayoutTransform = new ScaleTransform()
            {
                ScaleX = +1,
                ScaleY = -1
            };

            this.Zoom = 1.0d;

            this.scale = new ScaleTransform();
            this.translate = new TranslateTransform();

            this.ViewTransform = new TransformGroup();
            {
                ViewTransform.Children.Add(scale);
                ViewTransform.Children.Add(translate);
            }
        }

        #region Override Mouse Events

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            this.WorldPosition = ViewTransform.Inverse.Transform(e.GetPosition(this));

            if (this.IsMouseCaptured)
            {
                var currentPoint = e.GetPosition(this);
                translate.X = currentPoint.X - startPoint.X + startOffset.X;
                translate.Y = currentPoint.Y - startPoint.Y + startOffset.Y;
            }

            this.RubberLine.SetMove(WorldPosition);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            {
                if (e.Delta > 0)
                {
                    Zoom *= 1.2d;

                    if (Zoom >= 100d)
                    {
                        Zoom = 100d;
                    }
                }
                else
                {
                    Zoom /= 1.2d;

                    if (Zoom <= 0.001d)
                    {
                        Zoom = 0.001d;
                    }
                }
            }

            var scrPoint = e.GetPosition(this);
            var wrdPoint = ViewTransform.Inverse.Transform(scrPoint);

            translate.X = -1 * (wrdPoint.X * Zoom - scrPoint.X);
            translate.Y = -1 * (wrdPoint.Y * Zoom - scrPoint.Y);
            scale.ScaleX = Zoom;
            scale.ScaleY = Zoom;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    startPoint = e.GetPosition(this);
                    startOffset = new Point(translate.X, translate.Y);

                    this.CaptureMouse();
                    this.Cursor = Cursors.ScrollAll;
                }

                this.RubberLine.SetStart(WorldPosition);
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            {
                if (IsMouseCaptured)
                {
                    Cursor = Cursors.Cross;
                    ReleaseMouseCapture();
                }
            }
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            {

            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }

        #endregion

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            FrameworkElement This = visualAdded as FrameworkElement;

            if (This is ScreenLayer)
            {
                this.screen = This as ScreenLayer;
                this.RubberLine = (This as ScreenLayer).rubber;
            }

            if (This != null) 
                This.RenderTransform = ViewTransform;

            this.screen.SetValue(Canvas.ZIndexProperty, this.Children.Count - 1);
        }
    }
}
