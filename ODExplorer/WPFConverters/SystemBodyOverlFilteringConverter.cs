using ODExplorer.Controls;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.WPFConverters
{
    public sealed class SystemBodyOverlayFilteringConverter : IValueConverter
    {
        private GridFiltering target;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridFiltering mask = (GridFiltering)parameter;
            this.target = (GridFiltering)value;
            return ((mask & this.target) != 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.target ^= (GridFiltering)parameter;
            return this.target;
        }
    }
}
