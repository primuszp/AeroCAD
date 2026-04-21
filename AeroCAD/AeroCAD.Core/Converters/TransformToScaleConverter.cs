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
            if (value is ScaleTransform scaleTransform)
            {
                return System.Math.Abs(scaleTransform.ScaleX);
            }

            if (value is MatrixTransform matrixTransform)
            {
                return System.Math.Abs(matrixTransform.Matrix.M11);
            }

            if (value is Transform transform)
            {
                return System.Math.Abs(transform.Value.M11);
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
