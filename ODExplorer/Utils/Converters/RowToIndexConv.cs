using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class RowToIndexConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridRow row = value as DataGridRow;
            return row.GetIndex() + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}