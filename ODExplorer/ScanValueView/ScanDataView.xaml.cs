using ODExplorer.NavData;
using ODExplorer.Utils;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ODExplorer.ScanValueView
{
    /// <summary>
    /// Interaction logic for ScanDataView.xaml
    /// </summary>
    public partial class ScanDataView : Window
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
        public EstimatedScanValue ScanValue { get; private set; }
        public ScanDataView(EstimatedScanValue scanValue)
        {
            ScanValue = scanValue;
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;

            SystemInfo system = (SystemInfo)lb.SelectedItem;

            if (system is null || SystemDetails is null)
            {
                return;
            }

            SystemDetails.SortDataGrid(new SortDescription("BodyID", ListSortDirection.Ascending));
        }

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox lb && lb.HasItems)
            {
                lb.SelectedIndex = lb.Items.Count - 1;
                lb.ScrollIntoView(lb.SelectedItem);
            }
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = (e.Item as SystemBody).PlanetClass != EliteJournalReader.PlanetClass.EdsmValuableBody;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ScanValue.ScannedSystems.CollectionChanged += ScannedSystems_CollectionChanged;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ScanValue.ScannedSystems.CollectionChanged -= ScannedSystems_CollectionChanged;
        }

        private void ScannedSystems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SystemListBox is not null && SystemListBox.HasItems)
            {
                SystemListBox.SelectedIndex = SystemListBox.Items.Count - 1;
                SystemListBox.ScrollIntoView(SystemListBox.SelectedItem);
            }
        }
    }
}
