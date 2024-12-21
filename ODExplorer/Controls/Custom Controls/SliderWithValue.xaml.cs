using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for SliderWithValue.xaml
    /// </summary>
    public enum ValueDisplayFormat
    {
        ZeroDp,
        OneDp,
        TwoDp,
        ThreeDp
    }
    public partial class SliderWithValue : UserControl
    {
        public SliderWithValue()
        {
            InitializeComponent();
        }



        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SliderWithValue), new PropertyMetadata(0d));



        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SliderWithValue), new PropertyMetadata(100d));



        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register("SmallChange", typeof(double), typeof(SliderWithValue), new PropertyMetadata(1d));



        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LargeChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(double), typeof(SliderWithValue), new PropertyMetadata(1d));



        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SliderWithValue), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public double TickFrequency
        {
            get { return (double)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TickFrequency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(double), typeof(SliderWithValue), new PropertyMetadata(1d));



        public ValueDisplayFormat StringFormatting
        {
            get { return (ValueDisplayFormat)GetValue(StringFormattingProperty); }
            set { SetValue(StringFormattingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringFormatting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringFormattingProperty =
            DependencyProperty.Register("StringFormatting", typeof(ValueDisplayFormat), typeof(SliderWithValue), new PropertyMetadata());


        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch (StringFormatting)
            {
                case ValueDisplayFormat.ZeroDp:
                    ValueText.Text = $"{e.NewValue:N0}";
                    break;
                case ValueDisplayFormat.OneDp:
                    ValueText.Text = $"{e.NewValue:N1}";
                    break;
                case ValueDisplayFormat.TwoDp:
                    ValueText.Text = $"{e.NewValue:N2}";
                    break;
                case ValueDisplayFormat.ThreeDp:
                    ValueText.Text = $"{e.NewValue:N3}";
                    break;
                default:
                    break;
            };
        }
    }
}
