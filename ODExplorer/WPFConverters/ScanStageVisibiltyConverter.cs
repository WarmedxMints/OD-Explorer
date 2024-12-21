using EliteJournalReader;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ODExplorer.WPFConverters
{
    internal class ScanStageVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrganicScanStage stage && parameter is OrganicScanStage param)
            {
                var visible = param >= OrganicScanStage.Log ? stage >= param : stage == param;

                return visible ? Visibility.Visible : Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
