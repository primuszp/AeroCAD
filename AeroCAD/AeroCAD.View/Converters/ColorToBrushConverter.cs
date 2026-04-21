using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Primusz.AeroCAD.View.Converters
{
    public sealed class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush) || value is not Color color)
                return null;

            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
                brush.Freeze();

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
                return brush.Color;

            return Binding.DoNothing;
        }
    }
}
