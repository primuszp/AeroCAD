using System;
using System.Windows;
using System.Windows.Media;
using Primusz.Cadves.Core.Drawing.Entities;
using Primusz.Cadves.Core.Helpers;

namespace Primusz.Cadves.Core.Drawing.Handles
{
    public class Grip : DrawingVisual, ISelectable
    {
        #region Members

        private Brush brush;
        private Color color;

        #endregion

        #region Properties

        public Entity Owner { get; private set; }

        public bool IsSelected { get; private set; }

        public int Index { get; private set; }

        public double Size { get; set; }

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

        #endregion

        #region Constructors

        public Grip(Entity entity, int index)
        {
            Size = 10;
            Index = index;
            Owner = entity;
            Color = Colors.MediumBlue;
        }

        #endregion

        #region Methods

        public void Render()
        {
            Viewport viewport = VisualTreeHelpers.FindAncestor<Viewport>(this);

            if (viewport != null)
            {
                Point point = viewport.Project(Owner.GetGripPoint(Index));
                Rect rect = GetGripRect(point);

                using (DrawingContext dc = RenderOpen())
                {
                    dc.DrawRectangle(brush, new Pen(Brushes.LightGray, 1.5), rect);
                }
            }
        }

        private Rect GetGripRect(Point point)
        {
            return new Rect
            {
                X = point.X - Size / 2.0d,
                Y = point.Y - Size / 2.0d,
                Width = Size,
                Height = Size
            };
        }

        #endregion

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            Render();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            Viewport viewport = VisualTreeHelpers.FindAncestor<Viewport>(this);

            return base.HitTestCore(hitTestParameters);
        }

        #region From ISelectable interface

        public void Select()
        {
            IsSelected = true;
        }

        public void Unselect()
        {
            IsSelected = false;
        }

        #endregion
    }
}
