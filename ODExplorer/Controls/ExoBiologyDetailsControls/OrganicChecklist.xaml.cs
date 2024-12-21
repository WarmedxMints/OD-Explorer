using System.Windows.Controls;
using System.Windows.Input;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for OrganicChecklist.xaml
    /// </summary>
    public partial class OrganicChecklist : UserControl
    {
        public OrganicChecklist()
        {
            InitializeComponent();
        }

        private void Panels_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScrollBar.ScrollToVerticalOffset(MainScrollBar.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
