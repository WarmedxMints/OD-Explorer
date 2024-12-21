using ODExplorer.ViewModels.ModelVMs;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for OrganicChecklistTable.xaml
    /// </summary>
    public partial class OrganicChecklistTable : UserControl
    {
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(OrganicChecklistTable), new PropertyMetadata(string.Empty));



        public List<OrganicCheckListItemViewModel> Species
        {
            get { return (List<OrganicCheckListItemViewModel>)GetValue(SpeciesProperty); }
            set { SetValue(SpeciesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Species.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeciesProperty =
            DependencyProperty.Register("Species", typeof(List<OrganicCheckListItemViewModel>), typeof(OrganicChecklistTable), new PropertyMetadata());



        public OrganicCheckListItemViewModel SelectedItem
        {
            get { return (OrganicCheckListItemViewModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(OrganicCheckListItemViewModel), typeof(OrganicChecklistTable), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public OrganicChecklistTable()
        {
            InitializeComponent();
        }

        private void OrganicScanGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                DataGrid? grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    DataGridRow? dgr = grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem) as DataGridRow;
                    if (dgr is not null && !dgr.IsMouseOver)
                    {
                        (dgr as DataGridRow).IsSelected = false;
                    }
                }
            }
        }
    }
}
