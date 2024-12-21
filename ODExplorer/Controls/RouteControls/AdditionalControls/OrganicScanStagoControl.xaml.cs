using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for OrganicScanStageControl.xaml
    /// </summary>
    public partial class OrganicScanStageControl : UserControl
    {
        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(double), typeof(OrganicScanStageControl), new PropertyMetadata(25d));


        public OrganicScanStageControl()
        {
            InitializeComponent();
        }
    }
}
