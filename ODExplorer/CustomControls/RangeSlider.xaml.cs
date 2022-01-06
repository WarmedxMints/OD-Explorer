using System;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.CustomControls
{
    /// <summary>
    /// Interaction logic for RangeSlider.xaml
    /// </summary>
    public partial class RangeSlider : UserControl
    {
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double LowerValue
        {
            get => (double)GetValue(LowerValueProperty);
            set => SetValue(LowerValueProperty, value);
        }

        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register("LowerValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double UpperValue
        {
            get => (double)GetValue(UpperValueProperty);
            set => SetValue(UpperValueProperty, value);
        }

        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register("UpperValue", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(0d));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));

        public double TickFrequency
        {
            get => (double)GetValue(TickFrequencyProperty);
            set => SetValue(TickFrequencyProperty, value);
        }

        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(RangeSlider), new UIPropertyMetadata(1d));

        public string LowerInfoText
        {
            get => (string)GetValue(LowerInfoTextProperty);
            set => SetValue(LowerInfoTextProperty, value);
        }

        public static readonly DependencyProperty LowerInfoTextProperty =
            DependencyProperty.Register("LowerInfoText", typeof(string), typeof(RangeSlider), new UIPropertyMetadata());

        public string UpperInfoText
        {
            get => (string)GetValue(UpperInfoTextProperty);
            set => SetValue(UpperInfoTextProperty, value);
        }

        public static readonly DependencyProperty UpperInfoTextProperty =
            DependencyProperty.Register("UpperInfoText", typeof(string), typeof(RangeSlider), new UIPropertyMetadata());

        public Visibility InfoTextVisibility
        {
            get => (Visibility)GetValue(InfoTextVisibilityProperty);
            set => SetValue(InfoTextVisibilityProperty, value);
        }

        public static readonly DependencyProperty InfoTextVisibilityProperty =
            DependencyProperty.Register("InfoTextVisibility", typeof(Visibility), typeof(RangeSlider), new UIPropertyMetadata(Visibility.Visible));

        public string Title
        {
            get => (string)GetValue(TitleTextProperty);
            set => SetValue(TitleTextProperty, value);
        }

        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(RangeSlider), new UIPropertyMetadata());

        public string FormatString
        {
            get => (string)GetValue(FormatStringProperty);
            set => SetValue(FormatStringProperty, value);
        }

        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register("FormatString", typeof(string), typeof(RangeSlider), new UIPropertyMetadata("{0:N0}"));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(RangeSlider), new UIPropertyMetadata(false));

        public RangeSlider()
        {
            InitializeComponent();
            Loaded += RangeSlider_Loaded;
        }

        private void RangeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }

        private void LowerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpperSlider.Value = Math.Max(UpperSlider.Value, LowerSlider.Value);
        }

        private void UpperSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LowerSlider.Value = Math.Min(UpperSlider.Value, LowerSlider.Value);
        }
    }
}
