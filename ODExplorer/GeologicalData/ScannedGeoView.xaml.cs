using ODExplorer.CustomMessageBox;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ODExplorer.GeologicalData
{
    /// <summary>
    /// Interaction logic for ScannedGeoView.xaml
    /// </summary>
    public partial class ScannedGeoView : Window
    {
        #region Custom Title Bar
        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        // State change
        private void MainWindowStateChangeRaised(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MainWindowBorder.BorderThickness = new Thickness(8);
                RestoreButton.Visibility = Visibility.Visible;
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainWindowBorder.BorderThickness = new Thickness(0);
                RestoreButton.Visibility = Visibility.Collapsed;
                MaximizeButton.Visibility = Visibility.Visible;
            }
        }
        #endregion

        public ScannedGeoData ScannedGeo { get; set; }
        public ScannedGeoView(ScannedGeoData scannedGeoData)
        {
            ScannedGeo = scannedGeoData;
            InitializeComponent();
        }

        private void GeoDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid lb && lb.HasItems)
            {
                lb.SelectedIndex = lb.Items.Count - 1;
                lb.ScrollIntoView(lb.SelectedItem);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScannedGeo.ScannedData.CollectionChanged += ScannedData_CollectionChanged;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ScannedGeo.ScannedData.CollectionChanged -= ScannedData_CollectionChanged;
        }

        private void ScannedData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (GeoDataGrid is not null && GeoDataGrid.HasItems)
            {
                GeoDataGrid.SelectedIndex = GeoDataGrid.Items.Count - 1;
                GeoDataGrid.ScrollIntoView(GeoDataGrid.SelectedItem);
            }
        }

        private void DeleteBodyData_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;

            if (cmd.DataContext is GeoLogicalDataContainer deleteme)
            {
                MessageBoxResult result = ODMessageBox.Show(this,
                                                            $"Do you want to delete scan data for\n{deleteme.SystemName}?",
                                                            MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    ScannedGeo.ScannedData.RemoveFromCollection(deleteme);
                }
            }
        }

        private void GeoDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionViewSource viewSource = FindResource("ListCVS") as CollectionViewSource;

            viewSource.SortDescriptions.Clear();

            viewSource.SortDescriptions.Add(new System.ComponentModel.SortDescription("GeoName", System.ComponentModel.ListSortDirection.Ascending));

            viewSource.View?.Refresh();
        }

        private void SystemDetails_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //get all DataGridRow elements in the visual tree
            IEnumerable<DataGridRow> rows = FindVisualChildren<DataGridRow>(SystemDetails);

            foreach (DataGridRow row in rows)
            {
                row.Header = $"{row.GetIndex() + 1}.";
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject)
             where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void ClearAllData_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = ODMessageBox.Show(this,
                                                        "Do you want to clear all biological scan data?",
                                                        MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                ScannedGeo.ResetData();
            }
        }
    }
}
