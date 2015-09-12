using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using Primusz.Cadves.Core.Drawing.Entities;
using Primusz.Cadves.Core.Drawing.Layers;

namespace Primusz.Cadves.Core.Drawing
{
    public class Viewport : Canvas, IViewport
    {
        #region Properties

        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            "PropertyType", typeof(TranslateTransform), typeof(Viewport), new PropertyMetadata(default(TranslateTransform)));

        public TranslateTransform Translate
        {
            get { return (TranslateTransform)GetValue(PropertyTypeProperty); }
            set { SetValue(PropertyTypeProperty, value); }
        }

        public static readonly DependencyProperty ScaleTransformProperty = DependencyProperty.Register(
            "ScaleTransform", typeof(ScaleTransform), typeof(Viewport), new PropertyMetadata(default(ScaleTransform)));

        public ScaleTransform Scale
        {
            get { return (ScaleTransform)GetValue(ScaleTransformProperty); }
            set { SetValue(ScaleTransformProperty, value); }
        }

        public static readonly DependencyProperty ViewTransformProperty = DependencyProperty.Register(
            "ViewTransform", typeof(TransformGroup), typeof(Viewport), new PropertyMetadata(default(TransformGroup)));

        public TransformGroup ViewTransform
        {
            get { return (TransformGroup)GetValue(ViewTransformProperty); }
            set { SetValue(ViewTransformProperty, value); }
        }

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom", typeof(double), typeof(Viewport), new PropertyMetadata(default(double)));

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            "Position", typeof(Point), typeof(Viewport), new PropertyMetadata(default(Point)));

        public Point Position
        {
            get { return (Point)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        #endregion

        public Viewport()
        {
            ClipToBounds = true;
            LayoutTransform = new ScaleTransform() { ScaleX = 1, ScaleY = -1 };

            Scale = new ScaleTransform();
            Translate = new TranslateTransform();

            ViewTransform = new TransformGroup();
            {
                ViewTransform.Children.Add(Scale);
                ViewTransform.Children.Add(Translate);
            }
        }

        public void AddLayer(Layer layer)
        {
            Children.Add(layer);
            SetZIndex(layer, Children.Count);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            FrameworkElement element = visualAdded as FrameworkElement;

            if (element != null && !(element is Overlay))
            {
                element.RenderTransform = ViewTransform;
            }
        }

        /// <summary>
        /// Projects a point from object space into screen space. 
        /// </summary>
        /// <param name="point">World point</param>
        /// <returns></returns>
        public Point Project(Point point)
        {
            return ViewTransform.Transform(point);
        }

        /// <summary>
        /// Converts a screen space point into a corresponding point in world space. 
        /// </summary>
        /// <param name="point">Scree point</param>
        /// <returns></returns>
        public Point Unproject(Point point)
        {
            var inverse = ViewTransform.Inverse;
            return inverse != null ? inverse.Transform(point) : new Point();
        }

        public RubberObject GetRubberObject()
        {
            return Children.OfType<RubberObject>().FirstOrDefault();
        }

        public IList<Layer> GetLayers()
        {
            return Children.OfType<Layer>().ToList();
        }

        #region HitTest methods

        public bool HitTest(Point point)
        {
            bool retval = false;

            foreach (Layer layer in Children.OfType<Layer>())
            {
                layer.HitTest(point);

                if (layer.Selection.Count > 0)
                {
                    retval = true;
                }
            }

            return retval;
        }

        public bool HitTest(Rect rect)
        {
            bool retval = false;

            foreach (Layer layer in Children.OfType<Layer>())
            {
                layer.HitTest(rect);

                if (layer.Selection.Count > 0)
                {
                    retval = true;
                }
            }

            return retval;
        }

        #endregion
    }
}
