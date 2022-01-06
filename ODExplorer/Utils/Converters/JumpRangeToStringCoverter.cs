using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class JumpRangeToStringCoverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double i = (double)value;
            string val = i.ToString((string)parameter);

            return i <= 0.001 ? "?" : $"{val} ly";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}