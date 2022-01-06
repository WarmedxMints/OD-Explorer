using ODExplorer.NavData;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class RingNameConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SystemBody body = (SystemBody)value;

            int count = body.Rings.Length;

            if (count < 1)
            {
                return "";
            }

            if (body.IsStar)
            {
                return count < 2 ? "Belt :" : "Belts :";
            }

            return count < 2 ? "Ring :" : "Rings :";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}