using System;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.CustomControls
{
    /// <summary>
    /// Usage: Bind EnumFlag and Two way binding on EnumValue instead of IsChecked
    /// Example: <myControl:CheckBoxForEnumWithFlagAttribute 
    ///                 EnumValue="{Binding SimulationNatureTypeToCreateStatsCacheAtEndOfSimulation, Mode=TwoWay}" 
    ///                 EnumFlag="{x:Static Core:SimulationNatureType.LoadFlow }">Load Flow results</myControl:CheckBoxForEnumWithFlagAttribute>
    /// </summary>
    /// https://stackoverflow.com/a/22051021
    public class CheckBoxForEnumWithFlagAttribute : CheckBox
    {
        // ************************************************************************
        public static DependencyProperty EnumValueProperty =
            DependencyProperty.Register("EnumValue", typeof(object), typeof(CheckBoxForEnumWithFlagAttribute), new PropertyMetadata(EnumValueChangedCallback));

        public static DependencyProperty EnumFlagProperty =
            DependencyProperty.Register("EnumFlag", typeof(object), typeof(CheckBoxForEnumWithFlagAttribute), new PropertyMetadata(EnumFlagChangedCallback));

        // ************************************************************************
        public CheckBoxForEnumWithFlagAttribute()
        {
            Checked += CheckBoxForEnumWithFlag_Checked;
            Unchecked += CheckBoxForEnumWithFlag_Unchecked;
        }

        // ************************************************************************
        private static void EnumValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is CheckBoxForEnumWithFlagAttribute checkBoxForEnumWithFlag)
            {
                checkBoxForEnumWithFlag.RefreshCheckBoxState();
            }
        }

        // ************************************************************************
        private static void EnumFlagChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is CheckBoxForEnumWithFlagAttribute checkBoxForEnumWithFlag)
            {
                checkBoxForEnumWithFlag.RefreshCheckBoxState();
            }
        }

        // ************************************************************************
        public object EnumValue
        {
            get => GetValue(EnumValueProperty);
            set => SetValue(EnumValueProperty, value);
        }

        // ************************************************************************
        public object EnumFlag
        {
            get => GetValue(EnumFlagProperty);
            set => SetValue(EnumFlagProperty, value);
        }

        // ************************************************************************
        private void RefreshCheckBoxState()
        {
            if (EnumValue != null)
            {
                if (EnumValue is Enum)
                {
                    Type underlyingType = Enum.GetUnderlyingType(EnumValue.GetType());
                    dynamic valueAsInt = Convert.ChangeType(EnumValue, underlyingType);
                    dynamic flagAsInt = Convert.ChangeType(EnumFlag, underlyingType);

                    IsChecked = ((valueAsInt & flagAsInt) > 0);
                }
            }
        }

        // ************************************************************************
        private void CheckBoxForEnumWithFlag_Checked(object sender, RoutedEventArgs e)
        {
            RefreshEnumValue();
        }

        // ************************************************************************
        void CheckBoxForEnumWithFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshEnumValue();
        }

        // ************************************************************************
        private void RefreshEnumValue()
        {
            if (EnumValue != null)
            {
                if (EnumValue is Enum)
                {
                    Type underlyingType = Enum.GetUnderlyingType(EnumValue.GetType());
                    dynamic valueAsInt = Convert.ChangeType(EnumValue, underlyingType);
                    dynamic flagAsInt = Convert.ChangeType(EnumFlag, underlyingType);

                    dynamic newValueAsInt = valueAsInt;
                    if (IsChecked == true)
                    {
                        newValueAsInt = valueAsInt | flagAsInt;
                    }
                    else
                    {
                        newValueAsInt = valueAsInt & ~flagAsInt;
                    }

                    if (newValueAsInt != valueAsInt)
                    {
                        object o = Enum.ToObject(EnumValue.GetType(), newValueAsInt);

                        EnumValue = o;
                    }
                }
            }
        }

        // ************************************************************************
    }
}
