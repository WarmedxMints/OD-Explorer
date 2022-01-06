using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return (Visibility)Enum.Parse(typeof(Visibility), (string)parameter);
            }
            int val = (int)value;
            Visibility visibility = (Visibility)Enum.Parse(typeof(Visibility), (string)parameter);

            return val > 0 ? Visibility.Visible : visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}