using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class CarrierJumpsToTime : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int remaingCountSeconds = (((int)values[1]) + 1) * 1200;
            int timerSeconds = (int)values[2];

            string targetJumpSystem = values[3]?.ToString();
            string currentCsvSystem = values[0]?.ToString();

            int secondsLeft = string.Equals(currentCsvSystem, targetJumpSystem, StringComparison.OrdinalIgnoreCase)
            ? remaingCountSeconds - timerSeconds
            : remaingCountSeconds + (1200 - timerSeconds);

            TimeSpan t = TimeSpan.FromSeconds(secondsLeft);

            return $"Estimated Time Remaining : {t.Hours}h {t.Minutes}m{Environment.NewLine}ETA : {DateTime.Now + t:dddd - HH:mm}";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
