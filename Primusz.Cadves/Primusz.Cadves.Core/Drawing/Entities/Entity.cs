using System;
using System.Windows;
using System.Windows.Media;
using Primusz.Cadves.Core.Drawing.Handles;

namespace Primusz.Cadves.Core.Drawing.Entities
{
    public abstract class Entity : DrawingVisual, ISelectable
    {
        #region Members

        private bool selected;
        private double scale = 1.0;
        private double thickness = 1.0;

        #endregion

        #region Properties

        public double Scale
        {
            get { return scale; }
            set
            {
                scale = value;
                Pen.Thickness = thickness * scale;
            }
        }

        public double Thickness
        {
            get { return thickness; }
            set
            {
                thickness = value;
                Pen.Thickness = thickness * scale;
            }
        }

        public bool IsSelected { get; private set; }

        protected Pen Pen { get; set; }

        #endregion

        protected Entity()
        {
            Pen = new Pen();
        }

        public virtual void Render(Transform transform = null)
        {
            Pen.DashStyle = !IsSelected ? DashStyles.Solid : DashStyles.Dash;
        }

        #region From HitTest

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return base.HitTestCore(hitTestParameters);
        }

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            return base.HitTestCore(hitTestParameters);
        }

        #endregion

        /// <summary>
        /// Get grip point by index
        /// </summary>
        public abstract Point GetGripPoint(int index);


        #region From ISelectable interface

        public void Select()
        {
            IsSelected = true;
            Render();
        }

        public void Unselect()
        {
            IsSelected = false;
            Render();
        }

        #endregion
    }
}
