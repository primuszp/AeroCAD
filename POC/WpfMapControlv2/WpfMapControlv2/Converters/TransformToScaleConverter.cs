using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfMapControlv2.Converters
{
    class TransformToScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MatrixTransform transform = value as MatrixTransform;

            if (targetType == typeof(double) && transform != null)
            {
                double scaleX = Math.Abs(1.0d / transform.Matrix.M11);
                double scaleY = Math.Abs(1.0d / transform.Matrix.M22);
                double scale = Math.Sqrt((scaleX * scaleX + scaleY * scaleY) / 2.0d);

                return scale;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
