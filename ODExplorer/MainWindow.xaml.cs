using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ParserLibrary;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ODExplorer.NavData;
using ODExplorer.AppSettings;
using ODExplorer.Utils;
using ODExplorer.ScanValueView;
using ODExplorer.OrganicData;
using System.Threading.Tasks;
using System.Linq;
using ODExplorer.CsvControl;
using System.Media;
using System.Windows.Threading;

namespace ODExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Property Changed
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (Dispatcher.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                return;
            }

            Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
        #endregion

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

        private readonly JournalData _journalData = new();
        public Settings AppSettings { get; private set; } = new();
        //public ExplorationTargets Targets { get; private set; } = new();

        public CsvController CsvController { get; set; } = new();
        public NavigationData NavData { get; private set; } = new();

        #region Window Methods
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NavData.AppSettings = AppSettings;

            _journalData.StartWatcher(NavData);

            CsvController.OnCurrentTargetUpdated += Targets_OnCurrentTargetUpdated;

            _ = CsvController.LoadPreviousSession();

            UpdateLabel();

            TimerDisplay.Text = "00 : 00";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (NavData.ScanValue.SaveState() == false)
            {
                _ = MessageBox.Show("Error Saving Scan Value");
            }

            _ = NavData.ScannedBio.SaveState();
            CsvController.SaveState();
            _ = AppSettings.SaveSettings();
        }
        #endregion

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainScrollBar.ScrollToVerticalOffset(MainScrollBar.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        #region Current System Bodies DataGrid Methods
        //Save a reference to the DataGrid when it is loaded
        private DataGrid _currentSystemBodiesDataGrid;

        private void CurrentSystemBodies_Loaded(object sender, RoutedEventArgs e)
        {
            _currentSystemBodiesDataGrid = (DataGrid)sender;
            _currentSystemBodiesDataGrid.Items.IsLiveSorting = true;

            foreach (DatagridLayout layout in AppSettings.Value.DisplaySettings.CSBColumnOrder)
            {
                int count = _currentSystemBodiesDataGrid.Columns.Count == 0 ? 0 : _currentSystemBodiesDataGrid.Columns.Count - 1;

                DataGridColumn column = _currentSystemBodiesDataGrid.Columns.FirstOrDefault(x => (string)x.Header == layout.Header);

                if (column == default)
                {
                    continue;
                }

                column.DisplayIndex = (layout.DisplayIndex <= count) ? layout.DisplayIndex : count;
            }

            _currentSystemBodiesDataGrid.ColumnReordered += CurrentSystemBodiesDataGrid_ColumnReordered;
            SortCurrentSystemBodiesGrid();
        }

        private void CurrentSystemBodiesDataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            AppSettings.Value.DisplaySettings.CSBColumnOrder.Clear();

            foreach (DataGridColumn gridColumn in _currentSystemBodiesDataGrid.Columns)
            {
                AppSettings.Value.DisplaySettings.CSBColumnOrder.Add(new DatagridLayout() { Header = (string)gridColumn.Header, DisplayIndex = gridColumn.DisplayIndex });
            }
        }

        //Clear reference to Datagrid when it is unloaded
        private void CurrentSystemBodies_Unloaded(object sender, RoutedEventArgs e)
        {
            _currentSystemBodiesDataGrid.ColumnReordered -= CurrentSystemBodiesDataGrid_ColumnReordered;
            _currentSystemBodiesDataGrid = null;
        }
        //Not entirely happy with this, it seems a bit clumsy to me
        //However, it is the only way I can find so far of getting the Body Name and Distance columns to auto size
        //when there is a column with a * size.
        private void CurrentSystemBodies_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            if (_currentSystemBodiesDataGrid is null)
            {
                return;
            }

            foreach (DataGridColumn col in _currentSystemBodiesDataGrid.Columns)
            {
                DataGridLength width = col.Width;
                col.Width = 0;
                col.Width = width;
            }
        }
        //Method to apply sorting to the datagrids contents
        private void SortCurrentSystemBodiesGrid()
        {
            if (_currentSystemBodiesDataGrid is null)
            {
                return;
            }

            List<SortDescription> sortDescriptions = new();
            //Always put stars at the bottom of the list
            sortDescriptions.Add(new SortDescription("IsStar", ListSortDirection.Ascending));
            //Always put EDSM VB's at the top
            sortDescriptions.Add(new SortDescription("IsEDSMvb", ListSortDirection.Descending));

            switch (AppSettings.Value.SortCategory)
            {
                case SortCategory.Gravity:
                    sortDescriptions.Add(new SortDescription("SurfaceGravity", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.Distance:
                    sortDescriptions.Add(new SortDescription("DistanceFromArrivalLs", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.Type:
                    sortDescriptions.Add(new SortDescription("PlanetClass", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.Name:
                    sortDescriptions.Add(new SortDescription("BodyName", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.BodyId:
                    sortDescriptions.Add(new SortDescription("BodyID", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.BioSignals:
                    sortDescriptions.Add(new SortDescription("BiologicalSignals", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.GeoSignals:
                    sortDescriptions.Add(new SortDescription("GeologicalSignals", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.WorthMappingValue:
                    sortDescriptions.Add(new SortDescription("WorthMapping", AppSettings.Value.SortDirection));
                    sortDescriptions.Add(new SortDescription("MappedValue", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.WorthMappingDistance:
                    sortDescriptions.Add(new SortDescription("WorthMapping", AppSettings.Value.SortDirection));
                    sortDescriptions.Add(new SortDescription("DistanceFromArrivalLs", AppSettings.Value.SortDirection.Reverse()));
                    break;
                case SortCategory.Value:
                    sortDescriptions.Add(new SortDescription("MappedValue", AppSettings.Value.SortDirection));
                    break;
                case SortCategory.None:
                    break;
                default:
                    sortDescriptions.Add(new SortDescription("MappedValue", AppSettings.Value.SortDirection));
                    break;
            }

            _currentSystemBodiesDataGrid.SortDataGrid(sortDescriptions);
        }
        #endregion

        private void SystemsInRouteGrid_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            foreach (DataGridColumn col in SystemsInRouteGrid.Columns)
            {
                DataGridLength width = col.Width;
                col.Width = 0;
                col.Width = width;
            }
        }

        #region Top Menu Methods
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new(this);
            sw.Owner = this;

            if ((bool)sw.ShowDialog())
            {
                NavData.RefreshBodiesStatus();
                //Give the UI time to scale before sorting and resizing the datagrid
                _ = Task.Delay(500).ContinueWith(t => Dispatcher.Invoke(() => SortCurrentSystemBodiesGrid()));
            }
        }

        private void DisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            DisplaySettingsView displaySettings = new(AppSettings);
            displaySettings.Owner = this;

            if ((bool)displaySettings.ShowDialog())
            {
                CurrentSystemGrid.Items.Refresh();
            }
        }

        private void ImportCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = "Browse Csv Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "csv",
                Filter = "csv files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CsvParserReturn csv = CsvParser.ParseCsv(openFileDialog.FileName);

                if (csv is null)
                {
                    _ = MessageBox.Show("Unable to parse CSV");
                    return;
                }

                CsvController.ProcessCsv(csv);

                AppSettings.Value.ShowParser = !AppSettings.Value.ShowParser;

                UpdateLabel();
            }
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenScanData_Click(object sender, RoutedEventArgs e)
        {
            ScanDataView sdv = new(NavData.ScanValue)
            {
                Owner = this
            };
            sdv.Closed += ScanDataView_Closed;
            sdv.Show();

            OpenScanData.IsEnabled = false;
        }

        private void ScanDataView_Closed(object sender, EventArgs e)
        {
            OpenScanData.IsEnabled = true;
        }

        private void OpenAboutbox_Click(object sender, RoutedEventArgs e)
        {
            AboutBox.AboutBox about = new();
            about.Owner = this;
            about.Show();
        }

        private void OpenBioData_Click(object sender, RoutedEventArgs e)
        {
            BioDataView bdv = new(NavData.ScannedBio)
            {
                Owner = this
            };
            bdv.Closed += BioDataViewClosed;
            bdv.Show();

            OpenBioData.IsEnabled = false;
        }

        private void BioDataViewClosed(object sender, EventArgs e)
        {
            OpenBioData.IsEnabled = true;
        }
        #endregion

        #region CSV Parser Methods
        private void Label_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AppSettings.Value.ShowParser = !AppSettings.Value.ShowParser;

            UpdateLabel();
        }

        private void UpdateLabel()
        {
            bool show = AppSettings.Value.ShowParser;

            TimeSpan animationSpeed = TimeSpan.FromMilliseconds(300);

            Show_CSV_Parser.Content = show ? "Hide CSV Parser" : "Show CSV Parser";


            Storyboard arrowStoryboard = new();

            ScaleTransform scale = new(show ? -1.0 : 1.0, 1.0);
            CSV_Arrow.RenderTransform = scale;

            DoubleAnimation flipscale = new()
            {
                Duration = animationSpeed,
                From = show ? -1.0 : 1.0,
                To = show ? 1.0 : -1.0
            };

            arrowStoryboard.Children.Add(flipscale);

            Storyboard.SetTargetProperty(flipscale, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(flipscale, CSV_Arrow);

            arrowStoryboard.Begin();

            Storyboard gridStoryBoard = new();


            DoubleAnimation gridHeight = new()
            {
                Duration = animationSpeed,
                From = show ? 0 : CSV_Parser_Grid.ActualHeight,
                To = show ? 180 : 0
            };

            gridStoryBoard.Children.Add(gridHeight);

            Storyboard.SetTargetProperty(gridHeight, new PropertyPath("Height"));
            Storyboard.SetTarget(gridHeight, CSV_Parser_Grid);

            gridStoryBoard.Begin();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            CsvController.CurrentIndex++;
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            CsvController.CurrentIndex--;
        }

        private void Targets_OnCurrentTargetUpdated(object sender, CsvEventArgs e)
        {
            if (AppSettings.Value.AutoCopyCsvSystemToClipboard == false)
            {
                return;
            }

            CopySystemToClipboard(e.Target);
        }

        private void CopySystemToClipboard(ExplorationTarget e)
        {
            CSVParserStatus.Text = $"\'{e.SystemName}\' Copied To Clipboard";
            Clipboard.SetText(e.SystemName);

            ((Storyboard)FindResource("FadeOut")).Begin(CSVParserStatus);
        }

        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CopySystemToClipboard(CsvController.CurrentTarget);
        }

        private DispatcherTimer _fleetCarrierJumpTimer;
        private const int _timerCount = 1200;

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (_fleetCarrierJumpTimer is null)
            {
                _fleetCarrierJumpTimer = new()
                {
                    Interval = TimeSpan.FromSeconds(1)
                };

                int count = _timerCount;

                _fleetCarrierJumpTimer.Tick += (_, a) =>
                {
                    if (count-- <= 0)
                    {
                        EndTimer();
                    }
                    else
                    {
                        UpdateTime(count);
                    }
                };

                UpdateTime(count);
                TimerStartBtn.Content = "Cancel Timer";
                _fleetCarrierJumpTimer.Start();
                return;
            }

            EndTimer();
        }

        private void UpdateTime(int count)
        {
            TimerDisplay.Text = secondsToString(count);
        }

        private void EndTimer()
        {
            _fleetCarrierJumpTimer.Stop();
            SystemSounds.Beep.Play();
            _fleetCarrierJumpTimer = null;
            TimerStartBtn.Content = "Start Jump Timer";
            TimerDisplay.Text = "00 : 00";
        }

        private void Countdown(int count, TimeSpan interval, Action<int> ts)
        {
            System.Windows.Threading.DispatcherTimer dt = new()
            {
                Interval = interval
            };

            dt.Tick += (_, a) =>
            {
                if (count-- <= 0)
                {
                    dt.Stop();
                    SystemSounds.Beep.Play();
                }
                else
                {
                    ts(count);
                }
            };

            ts(count);
            dt.Start();
        }

        private string secondsToString(int pTime)
        {
            return $"{pTime / 60:00} : {pTime % 60:00}";
        }
        #endregion
    }
}