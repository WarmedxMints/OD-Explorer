using System;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    /// <summary>WPF converter for changing an enum value with the Flags attribute to/from an integer value.
    /// </summary>
    public class FlagsIntConverter : IValueConverter
    {
        /// <summary>Convert from the enum value to an integer value.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val = -1;
            if (value is Enum)
            {
                val = (int)value;
            }

            return val;
        }

        /// <summary>Convert from the integer value back to the enum value.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!targetType.IsEnum)
            {
                throw new InvalidCastException(targetType.Name + " is not an Enum type.");
            }

            object result = Enum.ToObject(targetType, (int)value);
            return result;
        }
    }
}
