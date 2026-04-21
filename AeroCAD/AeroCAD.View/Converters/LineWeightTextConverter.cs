using System;
using System.Globalization;
using System.Windows.Data;
using Primusz.AeroCAD.Core.Drawing.Layers;

namespace Primusz.AeroCAD.View.Converters
{
    public sealed class LineWeightTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return d.ToString("0.##", CultureInfo.InvariantCulture);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (value as string)?.Trim().Replace(',', '.');
            if (string.IsNullOrEmpty(text))
                return Binding.DoNothing;

            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                return Binding.DoNothing;

            // Clamp to the valid AutoCAD lineweight range (mm)
            return Math.Max(LineWeightPalette.StandardValues[0],
                            Math.Min(LineWeightPalette.StandardValues[LineWeightPalette.StandardValues.Length - 1], parsed));
        }
    }
}
