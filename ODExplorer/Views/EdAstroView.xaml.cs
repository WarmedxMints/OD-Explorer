using MdXaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }
}
