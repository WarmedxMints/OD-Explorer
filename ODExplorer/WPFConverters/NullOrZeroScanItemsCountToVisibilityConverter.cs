using ODExplorer.ViewModels.ModelVMs;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODExplorer.WPFConverters
{
    public enum ConverterDirection
    {
        Normal,
        Inverted
    }

    public sealed class NullOrZeroScanItemsCountToVisibilityConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not ConverterDirection direction)
                return Visibility.Visible;

            var showControl = direction == ConverterDirection.Normal ? Visibility.Visible : Visibility.Collapsed;
            var hideControl = direction == ConverterDirection.Normal ? Visibility.Collapsed : Visibility.Visible;

            if (value == null)
                return hideControl;

            if (value is SystemBodyViewModel)
            {
                return showControl;
            }

            return Visibility.Collapsed;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
