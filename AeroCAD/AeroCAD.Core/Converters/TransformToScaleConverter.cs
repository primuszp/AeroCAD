using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace Primusz.AeroCAD.Core.Converters
{
    class TransformToScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Transform transform = value as Transform;

            if (targetType == typeof(double) && transform != null)
            {
                double scaleX = Math.Abs(1.0d / transform.Value.M11);
                double scaleY = Math.Abs(1.0d / transform.Value.M22);
                double retval = Math.Sqrt((scaleX * scaleX + scaleY * scaleY) / 2.0d);

                return retval;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
