using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Primusz.Cadves.Core.Drawing.Entities;

namespace Primusz.Cadves.Core.Drawing.Layers
{
    public class Layer : VisualHost
    {
        #region Members

        private Brush brush;
        private Color color;

        #endregion

        #region Properties

        public Guid Id { get; private set; }

        public string LayerName { get; set; }

        public Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                {
                    color = value;
                    brush = new SolidColorBrush(color);
                }
            }
        }

        public IList Selection { get; private set; }

        public IList<Entity> Entities
        {
            get { return Visuals.OfType<Entity>().ToList(); }
        }

        #endregion

        #region Constructors

        public Layer()
        {
            Id = Guid.NewGuid();
            Color = Colors.White;
            Selection = new ArrayList();
        }

        #endregion

        public void Add(Entity entity)
        {
            if (!Visuals.Contains(entity))
            {
                Visuals.Add(entity);
            }
        }

        public void Remove(Entity entity)
        {
            if (Visuals.Contains(entity))
            {
                Visuals.Remove(entity);
            }
        }

        public void Clear()
        {
            Visuals.Clear();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            foreach (Entity entity in Visuals.OfType<Entity>())
            {
                entity.Render(Viewport.ViewTransform);
            }
        }

        protected override void ScaleUpdate()
        {
            foreach (Entity entity in Visuals.OfType<Entity>())
            {
                entity.Scale = Scale;
            }
        }

        #region HitTest

        public bool HitTest(Point point)
        {
            Rect rectangle = new Rect(point.X - 6 * Scale, point.Y - 6 * Scale, 12 * Scale, 12 * Scale);
            return HitTest(rectangle);
        }

        public bool HitTest(Rect rect)
        {
            Selection.Clear();

            Geometry rectangle = new RectangleGeometry(rect);
            VisualTreeHelper.HitTest(this, HitTestFilter, HitTestResult, new GeometryHitTestParameters(rectangle));

            return Selection.Count > 0;
        }

        // Return the result of the hit test to the callback.
        private HitTestResultBehavior HitTestResult(HitTestResult result)
        {
            Entity entity = result.VisualHit as Entity;

            if (entity != null && entity.Selected == false)
            {
                entity.Selected = true;
                Selection.Add(result.VisualHit);
            }

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        public virtual HitTestFilterBehavior HitTestFilter(DependencyObject dobj)
        {
            return dobj.GetType() == typeof(Layer)
                ? HitTestFilterBehavior.ContinueSkipSelf
                : HitTestFilterBehavior.Continue;
        }

        #endregion
    }
}
