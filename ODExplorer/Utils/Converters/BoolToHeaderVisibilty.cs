using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class BoolToHeaderVisibilty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            DataGridHeadersVisibility visibilty = (DataGridHeadersVisibility)Enum.Parse(typeof(DataGridHeadersVisibility), (string)parameter);

            return boolValue ? visibilty : DataGridHeadersVisibility.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}