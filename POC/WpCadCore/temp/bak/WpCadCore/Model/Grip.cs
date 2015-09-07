using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace WpCadCore.Model
{
    public class Grip : DrawingVisual
    {
        protected int handle;
        protected Brush brush;
        protected bool selected = false;
        protected const double size = 12.0d;

        public bool IsSelected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;

                if (selected)
                    this.brush = Brushes.Tomato;
                else
                    this.brush = Brushes.Blue;
            }
        }

        public Grip(int handle)
            : base()
        {
            this.handle = handle;
            this.IsSelected = false;
        }

        public void Refresh()
        {
            (this.Parent as EntityBase).RenderGrip(this);
        }

        public void MoveTo(Point point)
        {
        }

        public void Render(Transform transform)
        {
            Rect box = new Rect(-size / 2.0, -size / 2.0, size, size);
            IPoint pt = (Parent as EntityBase).GetHandlePoint(handle);

            using (DrawingContext dc = this.RenderOpen())
            {
                dc.PushTransform(transform);
                {
                    dc.DrawRectangle(brush, null, box);
                }
                dc.Pop();
            }
            this.Transform = new TranslateTransform(pt.X, pt.Y);
        }
    }
}
