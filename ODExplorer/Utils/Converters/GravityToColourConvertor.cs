using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ODExplorer.Utils.Converters
{
    public class GravityToColourConvertor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double i = (double)value;

            SolidColorBrush colour = new();

            if (i < 0.001)
            {
                colour.Color = Colors.Transparent;

                return colour;
            }
            if (i <= 0.8)
            {
                colour = (SolidColorBrush)Application.Current.Resources["LowGravity"];
                return colour;
            }
            if (i <= 1.2)
            {
                colour = (SolidColorBrush)Application.Current.Resources["MedGravity"];
                return colour;
            }

            colour = (SolidColorBrush)Application.Current.Resources["HighGravity"];
            return colour;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}