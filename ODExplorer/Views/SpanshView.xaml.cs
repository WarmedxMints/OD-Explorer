using ODExplorer.ViewModels.ViewVMs;
using ODExplorer.Windows;
using ODUtils.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace ODExplorer.Views
{
    /// <summary>
    /// Interaction logic for SpanshView.xaml
    /// </summary>
    public partial class SpanshView : UserControl
    {
        public SpanshView()
        {
            InitializeComponent();
            Loaded += SpanshView_Loaded;
            Unloaded += SpanshView_Unloaded;
        }

        private void SpanshView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel spanshViewModel)
            {
                spanshViewModel.OnErrorProcessingCSV -= SpanshViewModel_OnErrorProcessingCSV;
            }
        }

        private void SpanshView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel spanshViewModel)
            {
                spanshViewModel.OnErrorProcessingCSV += SpanshViewModel_OnErrorProcessingCSV;
            }
        }

        private void SpanshViewModel_OnErrorProcessingCSV(object? sender, Models.SpanshCsvErrorEventArgs e)
        {
            if (sender is not SpanshViewModel model)
                return;

            var owner = Window.GetWindow(this);
            if (e.ErrorType == Models.SpanshCSVError.Parse)
            {
                var selector = new SpanshCSVSelector()
                {
                    Owner = owner
                };
                selector.ShowDialog();

                if (selector.Result > ODUtils.Spansh.CsvType.None)
                {
                    model.ForceParseCSV(e.Filename, selector.Result);
                }
                return;
            }

            _ = ODMessageBox.Show(owner, "Unable to parse CSV", $"Error parsing {Path.GetFileName(e.Filename)}", MessageBoxButton.OK);
        }

        private void ImportCSVButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel spanshViewModel)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "Browse csv files",

                    CheckFileExists = true,
                    CheckPathExists = true,

                    DefaultExt = "csv",
                    Filter = "csv files (*.csv)|*.csv",
                    FilterIndex = 2,
                    RestoreDirectory = true,

                    ReadOnlyChecked = true,
                    ShowReadOnly = true
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    spanshViewModel.ParseCSV(openFileDialog.FileName);
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid grid && e.AddedItems.Count > 0)
            {
                grid.ScrollIntoView(e.AddedItems[0]);
            }
        }
    }
}
