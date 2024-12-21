using EliteJournalReader;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ODExplorer.WPFConverters
{
    public sealed class StarClassToScoopClassConverter : IValueConverter
    {
        object? IValueConverter.Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is StarType starType)
            {
                return starType switch
                {
                    StarType.K or StarType.G or StarType.B or StarType.F or StarType.O or StarType.A or StarType.M => Application.Current.Resources["ScoopableStar"] as SolidColorBrush,
                    StarType.N => Application.Current.Resources["NeutronStar"] as SolidColorBrush,
                    _ => Application.Current.Resources["NonScoopableStar"] as SolidColorBrush,
                };
            }
            return new SolidColorBrush(Colors.Black); ;
        }

        object? IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
