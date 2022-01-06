using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class KelvinToCelsiusFahrenheitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int kelvin = (int)value;

            return $"({kelvin - 273.15:N0} °C | {(kelvin - 273.15) * 9 / 5 + 32:N0} °F)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
