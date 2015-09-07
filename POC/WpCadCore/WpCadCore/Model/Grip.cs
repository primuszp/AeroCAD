using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace WpCadCore.Model
{
    public class Grip : DrawingVisual, ISelectable
    {
        protected int handle;
        protected bool selected = false;
        protected const double size = 12.0d;

        protected Pen pen = new Pen();
        protected Brush brush = Brushes.Blue;

        public bool IsSelected
        {
            get
            {
                return this.selected;
            }
            private set
            {
                this.selected = value;
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

                    if (IsSelected)
                        this.pen.Brush = Brushes.Red;
                    else
                        this.pen.Brush = Brushes.DarkGray;

                    dc.DrawRectangle(null, pen, box);
                }
                dc.Pop();
            }
            this.Transform = new TranslateTransform(pt.X, pt.Y);
        }

        #region ISelectable Members

        public void Select()
        {
            this.IsSelected = true;
            this.brush = Brushes.Red;
            this.Refresh();
        }

        public void Unselect()
        {
            this.IsSelected = false;
            this.brush = Brushes.Blue;
            this.Refresh();
        }

        #endregion
    }
}
