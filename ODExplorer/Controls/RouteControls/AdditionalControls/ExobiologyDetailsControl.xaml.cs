using ODExplorer.ViewModels.ModelVMs;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODExplorer.Controls.CartoDetailsControls
{
    /// <summary>
    /// Interaction logic for ExobiologyDetailsControl.xaml
    /// </summary>
    public partial class ExobiologyDetailsControl : UserControl
    {
        public ExobiologyDetailsControl()
        {
            InitializeComponent();
            Loaded += ExobiologyDetailsControl_Loaded;

        }

        private void ExobiologyDetailsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (SignalsGrid is not null)
            {
                SignalsGrid.Items.SortDescriptions.Clear();
                SignalsGrid.Items.SortDescriptions.Add(new(nameof(SystemBodyViewModel.DistanceFromArrival), System.ComponentModel.ListSortDirection.Ascending));
            }
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Grid is not null)
            {
                this.Grid.ScrollToVerticalOffset(Grid.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                if (e.AddedItems.Count > 0)
                {
                    grid.Items.Refresh();
                    grid.ScrollIntoView(e.AddedItems[0]);
                }
            }
        }
    }
}
