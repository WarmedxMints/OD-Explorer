using ParserLibrary;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class CsvTypeEnumToVis : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CsvType val = (CsvType)value;
            CsvType target = (CsvType)Enum.Parse(typeof(CsvType), (string)parameter);

            return val == target ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}