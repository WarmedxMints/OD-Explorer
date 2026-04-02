using ODExplorer.ViewModels.ModelVMs;
using ODExplorer.ViewModels.ViewVMs;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ODExplorer.Views
{
    /// <summary>
    /// Interaction logic for EdAstroView.xaml
    /// </summary>
    public partial class EdAstroView : UserControl
    {
        public EdAstroView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl(e.Uri.AbsoluteUri);
        }

        private void CopyToClipboard(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is EdAstroPoiViewModel poi && DataContext is EdAstroViewModel vm)
            {
                vm.CopyToClipboard(poi);
            }
        }
    }
}
