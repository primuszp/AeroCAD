using System;
using System.Windows;
using System.Windows.Media;
using Primusz.Cadves.Core.Drawing.Handles;

namespace Primusz.Cadves.Core.Drawing.Entities
{
    public abstract class Entity : DrawingVisual
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

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (value != selected)
                {
                    selected = value;
                    Render();
                }
            }
        }

        protected Pen Pen { get; set; }

        #endregion

        protected Entity()
        {
            Pen = new Pen();
        }

        public virtual void Render(Transform transform = null)
        {
            Pen.DashStyle = !Selected ? DashStyles.Solid : DashStyles.Dash;
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

        public abstract GripList GetGrips();

        /// <summary>
        /// Get grip point by index
        /// </summary>
        public abstract Point GetGripPoint(int index);
    }
}
