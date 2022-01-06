using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class StringToUpperCaseFirstCharacter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CultureInfo ci = new("en-GB", false);

            return value is string input ? ci.TextInfo.ToTitleCase(input.ToLower(ci)) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}