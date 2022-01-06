using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ODExplorer.Utils.Converters
{
    [ValueConversion(typeof(SolidColorBrush), typeof(SolidColorBrush))]
    internal class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush c = (SolidColorBrush)value;

            return c.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color col = (Color)value;

            return new SolidColorBrush(col);
        }

    }
}