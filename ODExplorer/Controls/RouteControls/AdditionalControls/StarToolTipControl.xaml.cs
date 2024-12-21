using ODExplorer.ViewModels.ModelVMs;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for StarToolTipControl.xaml
    /// </summary>
    public partial class StarToolTipControl : UserControl
    {
        public StarToolTipControl(SystemBodyViewModel systemBody)
        {
            DataContext = systemBody;
            InitializeComponent();
        }
    }
}
