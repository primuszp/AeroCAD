using System.Windows;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;
using Primusz.AeroCAD.Core.Drawing.Layers;
using Primusz.AeroCAD.Core.Drawing.Markers;

namespace Primusz.AeroCAD.Core.Drawing.Handles
{
    public class Grip : DrawingVisual, ISelectable
    {
        #region Members

        private Brush brush;
        private Color color;
        private readonly IMarkerAppearanceService appearanceService;
        private Viewport viewport; // cached reference, set in OnVisualParentChanged

        #endregion

        #region Properties

        public Entity Owner { get; private set; }

        public bool IsSelected { get; private set; }

        public int Index { get; private set; }

        public double Size => appearanceService?.MarkerSize ?? 10.0d;

        public GripKind Kind => Owner.GetGripKind(Index);

        public Color Color
        {
            get { return color; }
            set
            {
                if (!value.Equals(color))
                {
                    color = value;
                    brush = new SolidColorBrush(color);
                }
            }
        }

        #endregion

        #region Constructors

        public Grip(Entity entity, int index, IMarkerAppearanceService appearanceService)
        {
            Index = index;
            Owner = entity;
            this.appearanceService = appearanceService;
            Color = appearanceService?.GripEndpointColor ?? Colors.MediumBlue;
        }

        #endregion

        #region Methods

        public void Render()
        {
            if (viewport == null) return;

            Point point = viewport.Project(Owner.GetGripPoint(Index));
            Rect rect = GetGripRect(point);

            using (DrawingContext dc = RenderOpen())
            {
                Color = !IsSelected
                    ? (Kind == GripKind.Midpoint || Kind == GripKind.Center
                        ? appearanceService?.GripMidpointColor ?? Colors.Cyan
                        : appearanceService?.GripEndpointColor ?? Colors.MediumBlue)
                    : appearanceService?.GripActiveColor ?? Colors.Red;

                var borderBrush = new SolidColorBrush(appearanceService?.GripBorderColor ?? Colors.LightGray);
                if (borderBrush.CanFreeze)
                    borderBrush.Freeze();

                dc.DrawRectangle(brush, new Pen(borderBrush, appearanceService?.MarkerStrokeThickness ?? 1.5), rect);
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

            // Walk: Grip -> Overlay -> Viewport
            viewport = (VisualTreeHelper.GetParent(this) as Overlay)?.Viewport;
            Render();
        }

        #region ISelectable

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

