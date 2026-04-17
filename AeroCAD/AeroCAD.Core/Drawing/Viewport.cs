using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.Core.Drawing
{
    public class Viewport : Canvas, IViewport
    {
        private MatrixTransform worldTransform = new MatrixTransform();
        private Layers.CursorLayer cursorLayer;

        #region Properties

        public static readonly DependencyProperty TranslateProperty = DependencyProperty.Register(
            "Translate", typeof(TranslateTransform), typeof(Viewport), new PropertyMetadata(default(TranslateTransform)));

        public TranslateTransform Translate
        {
            get { return (TranslateTransform)GetValue(TranslateProperty); }
            set { SetValue(TranslateProperty, value); }
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

        public CadCursorType ActiveCursorType { get; set; } = CadCursorType.CrosshairAndPickbox;

        #endregion

        public Viewport()
        {
            ClipToBounds = true;

            Zoom = 1.0d;
            Scale = new ScaleTransform { ScaleX = 1.0d, ScaleY = 1.0d };
            Translate = new TranslateTransform();

            ViewTransform = new TransformGroup();
            ViewTransform.Children.Add(Scale);
            ViewTransform.Children.Add(Translate);

            Loaded += (s, e) => {
                EnsureCursorLayer();
                RefreshView();
            };
            SizeChanged += (s, e) => RefreshView();
        }

        public void AddLayer(Layer layer)
        {
            Children.Add(layer);
            SetZIndex(layer, Children.Count);
            ApplyCoordinateSpace(layer);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            FrameworkElement element = visualAdded as FrameworkElement;
            if (element != null)
                ApplyCoordinateSpace(element);
        }

        /// <summary>
        /// Projects a point from object space into screen space. 
        /// </summary>
        /// <param name="point">World point</param>
        /// <returns></returns>
        public Point Project(Point point)
        {
            return worldTransform.Transform(point);
        }

        /// <summary>
        /// Converts a screen space point into a corresponding point in world space. 
        /// </summary>
        /// <param name="point">Scree point</param>
        /// <returns></returns>
        public Point Unproject(Point point)
        {
            var inverse = worldTransform.Inverse;
            return inverse != null ? inverse.Transform(point) : new Point();
        }

        public void RefreshView()
        {
            if (Scale == null || Translate == null)
                return;

            Scale.ScaleX = Zoom;
            Scale.ScaleY = Zoom;

            worldTransform = new MatrixTransform(new Matrix(
                Zoom,
                0.0d,
                0.0d,
                -Zoom,
                Translate.X,
                ActualHeight + Translate.Y));

            foreach (FrameworkElement element in Children.OfType<FrameworkElement>())
            {
                UpdateHostedElementBounds(element);
                ApplyCoordinateSpace(element);
                element.InvalidateVisual();
            }
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

        public IList<Entity> QueryHitEntities(Point point)
        {
            var result = new List<Entity>();
            foreach (Layer layer in Children.OfType<Layer>())
                result.AddRange(layer.QueryHitEntities(point));
            return result;
        }

        public IList<Entity> QueryHitEntities(Point point, double toleranceWorld)
        {
            var result = new List<Entity>();
            foreach (Layer layer in Children.OfType<Layer>())
                result.AddRange(layer.QueryHitEntities(point, toleranceWorld));
            return result;
        }

        public IList<Entity> QueryHitEntities(Point point, IEnumerable<Entity> candidates)
        {
            var result = new List<Entity>();
            foreach (Layer layer in Children.OfType<Layer>())
                result.AddRange(layer.QueryHitEntities(point, candidates));
            return result;
        }

        public IList<Entity> QueryHitEntities(Point point, double toleranceWorld, IEnumerable<Entity> candidates)
        {
            var result = new List<Entity>();
            foreach (Layer layer in Children.OfType<Layer>())
                result.AddRange(layer.QueryHitEntities(point, toleranceWorld, candidates));
            return result;
        }

        public IList<Entity> QueryHitEntities(Rect rect, bool requireFullyInside = false)
        {
            var result = new List<Entity>();
            foreach (Layer layer in Children.OfType<Layer>())
                result.AddRange(layer.QueryHitEntities(rect, requireFullyInside));
            return result;
        }

        public IList<Entity> QueryHitEntities(Rect rect, bool requireFullyInside, IEnumerable<Entity> candidates)
        {
            var result = new List<Entity>();
            foreach (Layer layer in Children.OfType<Layer>())
                result.AddRange(layer.QueryHitEntities(rect, requireFullyInside, candidates));
            return result;
        }

        #endregion

        private void ApplyCoordinateSpace(FrameworkElement element)
        {
            if (element == null)
                return;

            if (GetCoordinateSpace(element) == ViewportCoordinateSpace.World)
                element.RenderTransform = worldTransform;
            else
                element.RenderTransform = Transform.Identity;
        }

        private static ViewportCoordinateSpace GetCoordinateSpace(FrameworkElement element)
        {
            var viewportElement = element as IViewportSpaceElement;
            return viewportElement?.CoordinateSpace ?? ViewportCoordinateSpace.World;
        }

        private void UpdateHostedElementBounds(FrameworkElement element)
        {
            var hostedElement = element as IViewportHostedElement;
            if (hostedElement == null)
                return;

            Canvas.SetLeft(element, 0.0d);
            Canvas.SetTop(element, 0.0d);
            hostedElement.UpdateViewportBounds(new Size(ActualWidth, ActualHeight));
        }

        private void EnsureCursorLayer()
        {
            if (cursorLayer != null)
                return;

            cursorLayer = new Layers.CursorLayer(this);
        }
    }
}

