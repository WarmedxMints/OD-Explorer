using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.WPFConverters
{
    public sealed class MaxHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is double height)
            {
                var currentHeight = (double)value;

                height = Math.Clamp(height, 0.0, 1.0);

                return currentHeight * height;
            }

            return (double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
