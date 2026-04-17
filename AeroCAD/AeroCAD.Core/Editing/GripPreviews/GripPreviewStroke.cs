using System;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Editing.GripPreviews
{
    public sealed class GripPreviewStroke
    {
        public Geometry Geometry { get; }

        public Color Color { get; }

        public double Thickness { get; }

        public DashStyle DashStyle { get; }

        public bool ScreenConstantThickness { get; }

        private GripPreviewStroke(Geometry geometry, Color color, double thickness, DashStyle dashStyle, bool screenConstantThickness)
        {
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            Color = color;
            Thickness = thickness;
            DashStyle = dashStyle ?? DashStyles.Solid;
            ScreenConstantThickness = screenConstantThickness;
        }

        public static GripPreviewStroke CreateScreenConstant(Geometry geometry, Color color, double thickness, DashStyle dashStyle = null)
        {
            return new GripPreviewStroke(geometry, color, thickness, dashStyle ?? DashStyles.Solid, true);
        }

        public static GripPreviewStroke CreateWorld(Geometry geometry, Color color, double thickness, DashStyle dashStyle = null)
        {
            return new GripPreviewStroke(geometry, color, thickness, dashStyle ?? DashStyles.Solid, false);
        }

        public Pen CreatePen(double zoom)
        {
            double effectiveZoom = zoom <= 0 ? 1.0d : zoom;
            double effectiveThickness = ScreenConstantThickness ? Thickness / effectiveZoom : Thickness;

            var brush = new SolidColorBrush(Color);
            if (brush.CanFreeze)
                brush.Freeze();

            var pen = new Pen(brush, effectiveThickness)
            {
                DashStyle = DashStyle
            };

            if (pen.CanFreeze)
                pen.Freeze();

            return pen;
        }
    }
}

