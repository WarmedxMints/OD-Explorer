using Microsoft.Win32;
using ODExplorer.CustomMessageBox;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ODExplorer.OrganicData
{
    /// <summary>
    /// Interaction logic for BioDataView.xaml
    /// </summary>
    public partial class BioDataView : Window
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

        public ScannedBioData ScannedBioData { get; private set; }

        public BioDataView(ScannedBioData scannedBioData)
        {
            ScannedBioData = scannedBioData;
            InitializeComponent();
        }

        private void BioDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid lb && lb.HasItems)
            {
                lb.SelectedIndex = lb.Items.Count - 1;
                lb.ScrollIntoView(lb.SelectedItem);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScannedBioData.ScannedData.CollectionChanged += ScannedData_CollectionChanged;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ScannedBioData.ScannedData.CollectionChanged -= ScannedData_CollectionChanged;
        }

        private void ScannedData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (BioDataGrid is not null && BioDataGrid.HasItems)
            {
                BioDataGrid.SelectedIndex = BioDataGrid.Items.Count - 1;
                BioDataGrid.ScrollIntoView(BioDataGrid.SelectedItem);
            }
        }

        private void DeleteBodyData_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;

            if (cmd.DataContext is BiologicalData deleteme)
            {
                MessageBoxResult result = ODMessageBox.Show(this,
                                                            $"Do you want to delete scan data for\n{deleteme.SystemName}?",
                                                            MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    ScannedBioData.ScannedData.RemoveFromCollection(deleteme);
                }
            }
        }

        private void BioDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CollectionViewSource viewSource = FindResource("ListCVS") as CollectionViewSource;

            viewSource.SortDescriptions.Clear();

            viewSource.SortDescriptions.Add(new System.ComponentModel.SortDescription("Species", System.ComponentModel.ListSortDirection.Ascending));

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
                ScannedBioData.ResetData();
            }
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = "Save CSV File",

                DefaultExt = "csv",
                Filter = "csv files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true,
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, ScannedBioData.GenerateCSV());
                }
                catch(Exception ex)
                {
                    _ = ODMessageBox.Show(this, $"{ex.Message}", "ERROR SAVING CSV");
                }
            }
        }
    }
}
