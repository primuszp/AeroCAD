using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Primusz.AeroCAD.Core.Converters
{
    class TransformToScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ScaleTransform transform)
            {
                return transform.ScaleX;
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
