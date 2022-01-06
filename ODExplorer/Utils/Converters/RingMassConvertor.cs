using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class RingMassConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double v = (double)values[0];
            bool IsStar = (bool)values[1];

            if (IsStar)
            {
                return v > 7347673090 ? $"{v * 1.360975083881964e-14:N4} Moons" : $"{v / 1000:N0} Km";
            }

            return $"{v / 1000:N0} Km";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}