using ODExplorer.ViewModels.ModelVMs;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for CurrentRouteControl.xaml
    /// </summary>
    public partial class CurrentRouteControl : UserControl
    {
        public CurrentRouteControl()
        {
            InitializeComponent();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
                dataGrid.ItemContainerGenerator.StatusChanged += (containerr, e) => ItemContainerGenerator_StatusChanged(containerr, dataGrid);
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
                dataGrid.ItemContainerGenerator.StatusChanged -= (containerr, e) => ItemContainerGenerator_StatusChanged(containerr, dataGrid);
        }

        private static void ItemContainerGenerator_StatusChanged(object? sender, DataGrid dataGrid)
        {
            if (sender is ItemContainerGenerator icg && icg.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (DataGridColumn col in dataGrid.Columns)
                {
                    DataGridLength width = col.Width;
                    col.Width = 0;
                    col.Width = width;
                }
            }
        }

        private void BodyDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                dataGrid.Items.Filter = NonSystemGridFilter;
                dataGrid.ItemContainerGenerator.StatusChanged += (containerr, e) => ItemContainerGenerator_StatusChanged(containerr, dataGrid);
            }
        }

        private void BodyDataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
                dataGrid.ItemContainerGenerator.StatusChanged -= (containerr, e) => ItemContainerGenerator_StatusChanged(containerr, dataGrid);

        }

        private bool NonSystemGridFilter(object obj)
        {
            if (obj is SystemBodyViewModel body)
            {
                return body.IsEdsmVb;
            }
            return false;
        }
    }
}
