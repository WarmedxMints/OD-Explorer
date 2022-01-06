using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class BoolToRowVisibilty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return boolValue ? DataGridRowDetailsVisibilityMode.Visible : DataGridRowDetailsVisibilityMode.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}