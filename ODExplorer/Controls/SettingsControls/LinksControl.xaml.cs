using System.Windows.Controls;
using System.Windows.Navigation;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for LinksControl.xaml
    /// </summary>
    public partial class LinksControl : UserControl
    {
        public LinksControl()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl(e.Uri.AbsoluteUri);
        }
    }
}
