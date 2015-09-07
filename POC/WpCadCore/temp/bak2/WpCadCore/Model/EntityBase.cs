using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace WpCadCore.Model
{
    public abstract class EntityBase : DrawingVisual, IEntity, ISelectable
    {
        #region Private members

        private Color normalColor;
        private double thickness;
        private bool selected;

        protected ScaleTransform scaleTransform = new ScaleTransform()
        {
            ScaleX = +1,
            ScaleY = +1
        };

        protected double scale = 1.0d;
        protected Pen pen = new Pen();
        
        #endregion

        #region Public properties

        public Color NormalColor
        {
            get
            {
                return this.normalColor;
            }
            set
            {
                this.normalColor = value;
                this.pen.Brush = new SolidColorBrush(normalColor);
            }
        }

        public double Thickness
        {
            get
            {
                return this.thickness;
            }
            set
            {
                this.thickness = value;
                this.pen.Thickness = this.thickness * scale;
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
            }
        }

        #endregion

        protected EntityBase(string name)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Thickness = 2.0d;
            this.IsSelected = false;
            this.NormalColor = Colors.White;
            this.Bounds = new BoundingBox(0, 0, 0, 0, 0, 0);
        }

        #region IEntity Members

        public IList<IPoint> Point3dCollection { get; set; }

        public BoundingBox Bounds { get; set; }

        public IPoint InitialPoint
        {
            get { return this.Point3dCollection[0]; }
        }

        public IPoint FinalPoint
        {
            get
            {
                return this.Point3dCollection.Count > 1 ? this.Point3dCollection[Point3dCollection.Count - 1] : InitialPoint;
            }
        }

        public string Name { get; private set; }

        public Guid Id { get; private set; }

        #endregion

        protected void ChechkHandle(ref int handle)
        {
            if (handle < 0)
                handle = 0;

            if (handle >= Point3dCollection.Count)
                handle = Point3dCollection.Count - 1;
        }

        public virtual void Refresh()
        {
            using (DrawingContext dc = this.RenderOpen())
            {
                this.Render(dc);
            }
        }

        public virtual void Render(DrawingContext dc)
        {
            if (!IsSelected)
                this.Children.Clear();
        }

        public virtual void ScaleUpdate(double scale)
        {
            this.scale = scale;
            this.scaleTransform.ScaleX = scale;
            this.scaleTransform.ScaleY = scale;
            this.pen.Thickness = this.Thickness * scale;
        }

        public abstract void PutGrips(List<Grip> grips);

        public abstract void RenderGrip(Grip grip);

        /// <summary>
        /// Move handle to the point
        /// </summary>
        public abstract void MoveHandleTo(IPoint point, int handle);

        /// <summary>
        /// Get handle point by number
        /// </summary>
        public abstract IPoint GetHandlePoint(int handle);

        public abstract void CalcBounds();

        #region ISelectable Members

        public void Select()
        {
            this.IsSelected = true;
            this.Refresh();
        }

        public void Unselect()
        {
            this.IsSelected = false;
            this.Refresh();
        }

        #endregion
    }
}
