using ODExplorer.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.WPFConverters
{
    public sealed class DistanceDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not Distance dist || value is not double v)
                throw new ArgumentException("Parameter is not Distance Enum");

            var distance = dist switch
            {
                Distance.Miles => $"{v * 0.62137:N1} mi",
                _ => $"{v:N0} km"
            };

            return distance;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
