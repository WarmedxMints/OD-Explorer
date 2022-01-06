using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class BoolToDatagridColoumnWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            bool boolValue = (bool)value;

            DataGridLengthUnitType width = boolValue ? DataGridLengthUnitType.SizeToCells : DataGridLengthUnitType.Star;

            return new DataGridLength(1, width);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}