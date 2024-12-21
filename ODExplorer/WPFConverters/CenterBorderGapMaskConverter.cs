using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace ODExplorer.WPFConverters
{
    public class CenterBorderGapMaskConverter : IMultiValueConverter
    {
        // Methods
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = typeof(double);
            if (values == null
                || values.Length != 3
                || values[0] == null
                || values[1] == null
                || values[2] == null
                || !type.IsAssignableFrom(values[0].GetType())
                || !type.IsAssignableFrom(values[1].GetType())
                || !type.IsAssignableFrom(values[2].GetType()))
            {
                return DependencyProperty.UnsetValue;
            }

            double pixels = (double)values[0];
            double width = (double)values[1];
            double height = (double)values[2];
            if (width == 0.0 || height == 0.0)
            {
                return null;
            }
            Grid visual = new()
            {
                Width = width,
                Height = height
            };
            ColumnDefinition colDefinition1 = new();
            ColumnDefinition colDefinition2 = new();
            ColumnDefinition colDefinition3 = new();
            colDefinition1.Width = new GridLength(1.0, GridUnitType.Star);
            colDefinition2.Width = new GridLength(pixels);
            colDefinition3.Width = new GridLength(1.0, GridUnitType.Star);
            visual.ColumnDefinitions.Add(colDefinition1);
            visual.ColumnDefinitions.Add(colDefinition2);
            visual.ColumnDefinitions.Add(colDefinition3);
            RowDefinition rowDefinition1 = new();
            RowDefinition rowDefinition2 = new();
            rowDefinition1.Height = new GridLength(height / 2.0);
            rowDefinition2.Height = new GridLength(1.0, GridUnitType.Star);
            visual.RowDefinitions.Add(rowDefinition1);
            visual.RowDefinitions.Add(rowDefinition2);
            Rectangle rectangle1 = new();
            Rectangle rectangle2 = new();
            Rectangle rectangle3 = new();
            rectangle1.Fill = Brushes.Black;
            rectangle2.Fill = Brushes.Black;
            rectangle3.Fill = Brushes.Black;
            Grid.SetRowSpan(rectangle1, 2);
            Grid.SetRow(rectangle1, 0);
            Grid.SetColumn(rectangle1, 0);
            Grid.SetRow(rectangle2, 1);
            Grid.SetColumn(rectangle2, 1);
            Grid.SetRowSpan(rectangle3, 2);
            Grid.SetRow(rectangle3, 0);
            Grid.SetColumn(rectangle3, 2);
            _ = visual.Children.Add(rectangle1);
            _ = visual.Children.Add(rectangle2);
            _ = visual.Children.Add(rectangle3);
            return new VisualBrush(visual);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return [Binding.DoNothing];
        }
    }
}
