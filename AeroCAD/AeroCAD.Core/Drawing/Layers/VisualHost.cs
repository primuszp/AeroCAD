using System;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Drawing.Layers
{
    public class VisualHost : ScaleFrameworkElement
    {
        #region Members

        protected VisualCollection Visuals;

        #endregion

        #region Properties

        public Viewport Viewport
        {
            get { return Parent as Viewport; }
        }

        #endregion

        public VisualHost()
        {
            Visuals = new VisualCollection(this);
        }

        protected override int VisualChildrenCount
        {
            get { return Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= Visuals.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return Visuals[index];
        }
    }
}
