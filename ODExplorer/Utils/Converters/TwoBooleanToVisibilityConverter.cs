using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class TwoBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool bool1 = (bool)values[0];
            bool bool2 = (bool)values[1];

            Visibility visibleType = bool2 ? Visibility.Hidden : Visibility.Collapsed;

            return bool1 && bool2 ? Visibility.Visible : visibleType;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}