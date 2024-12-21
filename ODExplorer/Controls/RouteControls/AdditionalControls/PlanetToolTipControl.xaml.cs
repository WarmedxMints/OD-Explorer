using ODExplorer.ViewModels.ModelVMs;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for PlanetToolTipControl.xaml
    /// </summary>
    public partial class PlanetToolTipControl : UserControl
    {
        public PlanetToolTipControl(SystemBodyViewModel bodyViewModel)
        {
            DataContext = bodyViewModel;
            InitializeComponent();
        }
    }
}
