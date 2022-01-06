using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class RingRadiusConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)values[0];
            bool IsStar = (bool)values[1];

            if (IsStar)
            {
                return v > 3000000 ? $"{v / 299792458:0.00} ls" : $"{v / 1000:N0} Km";
            }

            return $"{v / 1000:N0} Km";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}