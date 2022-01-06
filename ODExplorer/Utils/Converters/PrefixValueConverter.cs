using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class PrefixValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value.ToString();
            if (!int.TryParse(parameter.ToString(), out int prefixLength) ||
                s.Length <= prefixLength)
            {
                return s;
            }
            return s.Substring(0, prefixLength);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}