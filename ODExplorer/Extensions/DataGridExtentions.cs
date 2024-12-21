using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace ODExplorer.Extensions
{
    public static class DataGridExtensions
    {
        public static void SortDataGrid(this DataGrid dataGrid, List<SortDescription> sortDescriptions)
        {
            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();
            // Add the new sort descriptions
            foreach (SortDescription sort in sortDescriptions)
            {
                dataGrid.Items.SortDescriptions.Add(sort);
            }
            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        public static ListSortDirection Reverse(this ListSortDirection sortDirection)
        {
            return sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }
    }
}
