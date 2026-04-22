using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Primusz.AeroCAD.Core.Drawing.Entities;

namespace Primusz.AeroCAD.View.Converters
{
    public sealed class AciIndexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            byte index;
            if (value is byte b)
                index = b;
            else if (value is int i && i >= byte.MinValue && i <= byte.MaxValue)
                index = (byte)i;
            else
                return null;

            var brush = new SolidColorBrush(AciPalette.GetColor(index));
            if (brush.CanFreeze)
                brush.Freeze();

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
