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
            if(values.Length < 0)
            {
                return Visibility.Collapsed;
            }

            if (values[0] is bool bool1 && values[1] is bool bool2)
            {
                Visibility visibleType = bool2 ? Visibility.Hidden : Visibility.Collapsed;

                return bool1 && bool2 ? Visibility.Visible : visibleType;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}