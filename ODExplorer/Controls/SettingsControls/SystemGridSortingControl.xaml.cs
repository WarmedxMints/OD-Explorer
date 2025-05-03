using ODExplorer.ViewModels.ViewVMs;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for SystemGridSortingControl.xaml
    /// </summary>
    public partial class SystemGridSortingControl : UserControl
    {
        public SystemGridSortingControl()
        {
            InitializeComponent();
        }

        private void SliderWithValue_ValueChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if(DataContext is SettingsViewModel model)
            {
                model.OnExoMinValueChanged();
            }
        }
    }
}
