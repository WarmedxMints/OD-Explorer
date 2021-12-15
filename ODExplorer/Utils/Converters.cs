using ParserLibrary;
using EliteJournalReader;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace ODExplorer.Utils
{
    public class IntToStringCoverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int i = (int)value;

            return $"{i:N0}";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class JumpRangeToStringCoverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double i = (double)value;
            string val = i.ToString((string)parameter);

            return i <= 0.001 ? "?" : $"{val} ly";
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class GravityToColourConvertor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double i = (double)value;

            SolidColorBrush colour = new();

            if (i < 0.001)
            {
                colour.Color = Colors.Transparent;

                return colour;
            }
            if (i <= 0.8)
            {
                colour = (SolidColorBrush)Application.Current.Resources["Low Gravity"];
                return colour;
            }
            if (i <= 1.2)
            {
                colour = (SolidColorBrush)Application.Current.Resources["Med Gravity"];
                return colour;
            }

            colour = (SolidColorBrush)Application.Current.Resources["High Gravity"];
            return colour;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = (int)value;
            Visibility visibility = (Visibility)Enum.Parse(typeof(Visibility), (string)parameter);

            return val > 0 ? Visibility.Visible : visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = (int)value;

            return val > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        private enum Parameters
        {
            Normal, Inverted
        }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            Parameters direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);

            if (direction == Parameters.Inverted)
            {
                return !boolValue ? Visibility.Visible : Visibility.Hidden;
            }

            return boolValue ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EnumDescriptionConverter : IValueConverter
    {
        public static string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
                return attrib.Description;
            }
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum myEnum = (Enum)value;
            if (myEnum == null)
            {
                return null;
            }
            string description = GetEnumDescription(myEnum);

            if (parameter is not null)
            {
                _ = int.TryParse(parameter.ToString(), out int prefixLength);

                if (prefixLength > 0)
                {
                    if (description.Length <= prefixLength)
                    {
                        return description;
                    }
                    return description.Substring(0, prefixLength);
                }
            }
            return !string.IsNullOrEmpty(description) ? description : myEnum.ToString();
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public class CenterBorderGapMaskConverter : IMultiValueConverter
    {
        // Methods
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
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
            if ((width == 0.0) || (height == 0.0))
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
            return new object[] { Binding.DoNothing };
        }
    }

    public class RowToIndexConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridRow row = value as DataGridRow;
            return row.GetIndex() + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToHeaderVisibilty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;
            DataGridHeadersVisibility visibilty = (DataGridHeadersVisibility)Enum.Parse(typeof(DataGridHeadersVisibility), (string)parameter);

            return boolValue ? visibilty : DataGridHeadersVisibility.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToRowVisibilty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return boolValue ? DataGridRowDetailsVisibilityMode.Visible : DataGridRowDetailsVisibilityMode.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TwoBooleanToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool bool1 = (bool)values[0];
            bool bool2 = (bool)values[1];

            Visibility visibleType = bool2 ? Visibility.Hidden : Visibility.Collapsed;

            return bool1 && bool2 ? Visibility.Visible : visibleType;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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

    public class BoolToDouble : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            double ret = boolValue ? double.Parse((string)parameter) : 0;

            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = (bool)value;

            string parameterString = parameter as string;
            string[] parameters = parameterString.Split(new char[] { '|' });

            return boolValue ? new Thickness(double.Parse(parameters[0]), double.Parse(parameters[1]), double.Parse(parameters[2]), double.Parse(parameters[3])) : new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

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

    public class PrefixValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = value.ToString();
            if (!int.TryParse(parameter.ToString(), out int prefixLength) ||
                s.Length <= prefixLength)
            {
                return s;
            }
            return s.Substring(0, prefixLength);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class PlanetClassToImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PlanetClass PlanetClass = (PlanetClass)value;

            string uri;
            switch (PlanetClass)
            {
                case PlanetClass.MetalRichBody:
                case PlanetClass.HighMetalContentBody:
                case PlanetClass.RockyBody:
                case PlanetClass.IcyBody:
                case PlanetClass.RockyIceBody:
                    uri = "../Images/planet.png";
                    break;

                case PlanetClass.EarthlikeBody:
                    uri = "../Images/earth.png";
                    break;

                case PlanetClass.WaterWorld:
                case PlanetClass.WaterGiant:
                case PlanetClass.WaterGiantWithLife:
                    uri = "../Images/water.png";
                    break;

                case PlanetClass.AmmoniaWorld:
                case PlanetClass.GasGiantWithWaterBasedLife:
                case PlanetClass.GasGiantWithAmmoniaBasedLife:
                case PlanetClass.SudarskyClassIGasGiant:
                case PlanetClass.SudarskyClassIIGasGiant:
                case PlanetClass.SudarskyClassIIIGasGiant:
                case PlanetClass.SudarskyClassIVGasGiant:
                case PlanetClass.SudarskyClassVGasGiant:
                case PlanetClass.HeliumRichGasGiant:
                case PlanetClass.HeliumGasGiant:
                    uri = "../Images/jupiter.png";
                    break;

                case PlanetClass.EdsmValuableBody:
                case PlanetClass.Unknown:
                default:
                    uri = "../Images/language.png";
                    break;
            }

            return uri;
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

