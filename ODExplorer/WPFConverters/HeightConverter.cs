using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.WPFConverters
{
    public sealed class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(parameter as string, out var height))
            {
                var currentHeight = (double)value;

                return currentHeight - height;
            }

            return (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
