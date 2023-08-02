using Microsoft.Win32;
using ODExplorer.CustomControls;
using ODExplorer.CustomMessageBox;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ODExplorer.OrganicData
{
    /// <summary>
    /// Interaction logic for BioDataTable.xaml
    /// </summary>
    public partial class BioDataTable : UserControl
    {
        public BioDataTable()
        {
            InitializeComponent();
        }

        public ScannedBioData BioDataInstance
        {
            get => (ScannedBioData)GetValue(ScannedBioDataProperty);
            set => SetValue(ScannedBioDataProperty, value);
        }

        public static readonly DependencyProperty ScannedBioDataProperty =
            DependencyProperty.Register("BioDataInstance", typeof(ScannedBioData), typeof(BioDataTable), new UIPropertyMetadata());

        private void DeleteBodyData_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = (Button)sender;

            if (cmd.DataContext is BiologicalData deleteme)
            {
                MessageBoxResult result = ODMessageBox.Show(Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive),
                                                            $"Do you want to delete scan data for\n{deleteme.SystemName}?",
                                                            MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    BioDataInstance.ScannedData.RemoveFromCollection(deleteme);
                    BioDataInstance.UpdateTotalValue();
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
            MessageBoxResult result = ODMessageBox.Show(Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive),
                                                        "Do you want to clear all biological scan data?",
                                                        MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                BioDataInstance.ResetData();
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
                    File.WriteAllText(saveFileDialog.FileName, BioDataInstance.GenerateCSV());
                }
                catch (Exception ex)
                {
                    _ = ODMessageBox.Show(Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive), $"{ex.Message}", "ERROR SAVING CSV");
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (BioDataInstance is not null)
            {
                BioDataInstance.ScannedData.CollectionChanged += ScannedData_CollectionChanged;
                BioDataInstance.OnApproachBodyEvent += BioDataInstance_OnApproachBodyEvent;
            }
        }

        private void BioDataInstance_OnApproachBodyEvent(object sender, EliteJournalReader.Events.ApproachBodyEvent.ApproachBodyEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (BioDataGrid is not null && BioDataGrid.HasItems)
                {
                    var body = BioDataInstance.ScannedData.FirstOrDefault(x => e.Body.StartsWith(x.SystemName, StringComparison.OrdinalIgnoreCase) && e.Body.EndsWith(x.BodyName, StringComparison.OrdinalIgnoreCase));
                    if (body == null)
                    {
                        return;
                    }
                    var index = BioDataGrid.Items.IndexOf(body);

                    if (index >= 0)
                    {
                        BioDataGrid.SelectedIndex = index;
                        BioDataGrid.ScrollIntoView(BioDataGrid.SelectedItem);
                    }
                }
            });
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (BioDataInstance is not null)
            {
                BioDataInstance.ScannedData.CollectionChanged -= ScannedData_CollectionChanged;
                BioDataInstance.OnApproachBodyEvent -= BioDataInstance_OnApproachBodyEvent;
            }
        }

        private void ScannedData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (BioDataGrid is not null && BioDataGrid.HasItems)
            {
                BioDataGrid.SelectedIndex = BioDataGrid.Items.Count - 1;
                BioDataGrid.ScrollIntoView(BioDataGrid.SelectedItem);
            }
        }

        private void BioDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid lb && lb.HasItems)
            {
                lb.SelectedIndex = lb.Items.Count - 1;
                lb.ScrollIntoView(lb.SelectedItem);
            }
        }
    }
}
