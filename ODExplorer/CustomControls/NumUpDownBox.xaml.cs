using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODExplorer.CustomControls
{
    /// <summary>
    /// Interaction logic for NumUpDownBox.xaml
    /// </summary>
    public partial class NumUpDownBox : UserControl
    {
        public double ButtonStep
        {
            get => (double)GetValue(ButtonStepProperty);
            set => SetValue(ButtonStepProperty, value);
        }

        public static readonly DependencyProperty ButtonStepProperty =
            DependencyProperty.Register("ButtonStep", typeof(double), typeof(NumUpDownBox), new UIPropertyMetadata(1d));

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(NumUpDownBox), new UIPropertyMetadata(0d));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(NumUpDownBox), new UIPropertyMetadata(1d));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumUpDownBox), new UIPropertyMetadata(0d));

        public string InfoText
        {
            get => (string)GetValue(InfoTextProperty);
            set => SetValue(InfoTextProperty, value);
        }

        public static readonly DependencyProperty InfoTextProperty =
            DependencyProperty.Register("InfoText", typeof(string), typeof(NumUpDownBox), new UIPropertyMetadata());

        public Visibility InfoTextVisibility
        {
            get => (Visibility)GetValue(InfoTextVisibilityProperty);
            set => SetValue(InfoTextVisibilityProperty, value);
        }

        public static readonly DependencyProperty InfoTextVisibilityProperty =
            DependencyProperty.Register("InfoTextVisibility", typeof(Visibility), typeof(NumUpDownBox), new UIPropertyMetadata(Visibility.Visible));

        public string FormatString
        {
            get => (string)GetValue(FormatStringProperty);
            set => SetValue(FormatStringProperty, value);
        }

        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register("FormatString", typeof(string), typeof(NumUpDownBox), new UIPropertyMetadata("{0:N0}"));

        public NumUpDownBox()
        {
            InitializeComponent();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Math.Clamp(Value - ButtonStep,Minimum, Maximum);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Math.Clamp(Value + ButtonStep, Minimum, Maximum);
        }

        private readonly Regex _regex = new("[^0-9.-]+"); //regex that matches disallowed text
        private bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void DisplayBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void DisplayBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is Key.Return or Key.Enter)
            {
                double value = double.Parse(DisplayBox.Text);
                value = Math.Round(value / ButtonStep) * ButtonStep;
                value = Math.Clamp(value, Minimum, Maximum);
                Value = value;
                e.Handled = true;
            }
 
        }
    }
}
