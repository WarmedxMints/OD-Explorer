using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class BoolToMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            string parameterString = parameter as string;
            string[] parameters = parameterString.Split(new char[] { '|' });

            return boolValue ? new Thickness(double.Parse(parameters[0]), double.Parse(parameters[1]), double.Parse(parameters[2]), double.Parse(parameters[3])) : new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}