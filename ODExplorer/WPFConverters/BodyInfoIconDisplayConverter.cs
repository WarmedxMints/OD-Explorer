using ODExplorer.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.WPFConverters
{
    class BodyInfoIconDisplayConverter : IValueConverter
    {
        private BodyInfoIconDisplay target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BodyInfoIconDisplay mask = (BodyInfoIconDisplay)parameter;
            this.target = (BodyInfoIconDisplay)value;
            return ((mask & this.target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.target ^= (BodyInfoIconDisplay)parameter;
            return this.target;
        }
    }
}
