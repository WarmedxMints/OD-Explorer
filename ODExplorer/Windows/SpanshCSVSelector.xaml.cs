using ODUtils.Commands;
using ODUtils.Spansh;
using ODUtils.Windows;
using System.Windows.Input;

namespace ODExplorer.Windows
{
    /// <summary>
    /// Interaction logic for SpanshCSVSelector.xaml
    /// </summary>
    public partial class SpanshCSVSelector : WindowBase
    {
        public ICommand SetCSVType { get; }
        public CsvType Result { get; private set; } = CsvType.None;
        public SpanshCSVSelector()
        {
            InitializeComponent();
            SetAsToolWindow();

            SetCSVType = new RelayCommand<CsvType>(SelectCSVType);
        }

        private void SelectCSVType(CsvType csvType)
        {
            Result = csvType;
            DialogResult = true;
        }

        private void Image_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://www.spansh.co.uk/plotter");
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
