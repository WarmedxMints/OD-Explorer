using EliteJournalReader.Events;
using Microsoft.Win32;
using ODExplorer.AppSettings;
using ODExplorer.CsvControl;
using ODExplorer.CustomMessageBox;
using ODExplorer.GeologicalData;
using ODExplorer.NavData;
using ODExplorer.OrganicData;
using ODExplorer.ScanValueView;
using ODExplorer.Utils;
using ParserLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            if (AppSettings is not null && WindowState != WindowState.Minimized)
            {
                AppSettings.Value.LastWindowPos.State = WindowState;
            }

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

        private JournalData _journalData;
        public Settings AppSettings { get; private set; } = new();

        public CsvController CsvController { get; set; } = new();
        public NavigationData NavData { get; private set; } = new();

        private bool showCurrentSystemTable;
        public bool ShowCurrenSystemTable { get => showCurrentSystemTable; set { showCurrentSystemTable = value; OnPropertyChanged(); } }

        #region Window Methods
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = AppSettings.Value.LastWindowPos.State;

            _journalData = new(AppSettings);

            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            NavData.CurrentSystem.CollectionChanged += CurrentSystem_CollectionChanged;

            NavData.AppSettings = AppSettings;

            _journalData.StartWatcher(NavData);

            CsvController.OnCurrentTargetUpdated += Targets_OnCurrentTargetUpdated;

            _ = CsvController.LoadPreviousSession();

            UpdateLabel();

            NavData.OnCurrentSystemChanged += NavData_OnCurrentSystemChanged;

            _journalData.GetEvent<CarrierJumpRequestEvent>()?.AddHandler(CarrierJumpRequest);

            CountDownTimer.CountDownFinishedEvent += CountDownTimer_CountDownFinishedEvent;
        }


        private void NavData_OnCurrentSystemChanged(SystemInfo systemInfo)
        {
            Dispatcher.Invoke(() =>
            {
                if (AppSettings.Value.AutoSelectNextCsvSystem)
                {
                    CsvController.NavData_OnCurrentSystemChanged(systemInfo);
                }
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CountDownTimer.Stop();
            if (NavData.ScanValue.SaveState() == false)
            {
                _ = ODMessageBox.Show(this,
                                      "Error Saving Scan Value");
            }

            Settings.SettingsInstance.SaveAll();
            CsvController.SaveState();
            _ = AppSettings.SaveSettings();
        }
        #endregion
        private void CurrentSystem_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ShowCurrenSystemTable = NavData.CurrentSystem.Count > 0;
        }

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

            _currentSystemBodiesDataGrid.ItemContainerGenerator.StatusChanged += (sender, e) => ItemContainerGenerator_StatusChanged(sender, _currentSystemBodiesDataGrid);

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
            _currentSystemBodiesDataGrid.ItemContainerGenerator.StatusChanged -= (sender, e) => ItemContainerGenerator_StatusChanged(sender, _currentSystemBodiesDataGrid);
            _currentSystemBodiesDataGrid.ColumnReordered -= CurrentSystemBodiesDataGrid_ColumnReordered;
            _currentSystemBodiesDataGrid = null;
        }

        //Method to apply sorting to the datagrids contents
        private void SortCurrentSystemBodiesGrid()
        {
            if (_currentSystemBodiesDataGrid is null)
            {
                return;
            }

            List<SortDescription> sortDescriptions = new();
            if (AppSettings.Value.ExcludeStarsFromSorting)
            {
                //Always put stars at the bottom of the list
                sortDescriptions.Add(new SortDescription("IsStar", ListSortDirection.Ascending));
            }
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

        #region Top Menu Methods
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new(this);
            sw.Owner = this;

            if ((bool)sw.ShowDialog())
            {
                NavData.RefreshBodiesStatus();
                //Give the UI time to scale before sorting and resizing the datagrid
                _ = Task.Delay(200).ContinueWith(t => Dispatcher.Invoke(() => SortCurrentSystemBodiesGrid()));
            }
        }

        private void DisplaySettings_Click(object sender, RoutedEventArgs e)
        {
            DisplaySettingsView displaySettings = new(AppSettings);
            displaySettings.Owner = this;

            if ((bool)displaySettings.ShowDialog())
            {
                NavData.RefreshBodiesStatus();
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
                    _ = ODMessageBox.Show(this,
                                          "Unable to parse CSV");
                    return;
                }

                CsvController.ProcessCsv(csv);

                if (AppSettings.Value.ShowParser == false)
                {
                    AppSettings.Value.ShowParser = true;
                    UpdateLabel();
                }
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
            _ = about.ShowDialog();
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

        private void OpenGeoData_Click(object sender, RoutedEventArgs e)
        {
            ScannedGeoView gdv = new(NavData.ScannedGeo)
            {
                Owner = this
            };
            gdv.Closed += GeoDataViewClosed;
            gdv.Show();

            OpenGeoData.IsEnabled = false;
        }

        private void GeoDataViewClosed(object sender, EventArgs e)
        {
            OpenGeoData.IsEnabled = true;
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
            try
            {
                CSVParserStatus.Text = $"\'{e.SystemName}\' Copied To Clipboard";
                Clipboard.SetDataObject(e.SystemName);
                ((Storyboard)FindResource("FadeOut")).Begin(CSVParserStatus);
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Erroe sending System Name to Clipboard\nError : {ex.Message}");
            }
        }

        private void Rectangle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CopySystemToClipboard(CsvController.CurrentTarget);
        }

        private void CountDownTimer_CountDownFinishedEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                SystemSounds.Beep.Play();

                taskBarItem.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
                _ = Task.Run(async() =>
                  {
                      await Task.Delay(6000);
                      Dispatcher.Invoke(() =>
                      {
                          taskBarItem.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                      });
                  });
            });
        }

        private void CarrierJumpRequest(object sender, CarrierJumpRequestEvent.CarrierJumpRequestEventArgs e)
        {
            if (_journalData.WatcherLive == false || AppSettings.Value.AutoStartFleetCarrierTimer == false)
            {
                return;
            }

            Dispatcher.Invoke(() =>
            {
                StartTimer();
                CarrierJumpSystem = e.SystemName;
            });
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (CountDownTimer.TimerRunning)
            {
                CountDownTimer.Stop();
                TimerStartBtn.Content = "Start Jump Timer";
                return;
            }

            StartTimer();
        }

        private string carrierJumpSystem;
        public string CarrierJumpSystem { get => carrierJumpSystem; set { carrierJumpSystem = value; OnPropertyChanged(); } }

        private Countdown countDownTimer = new(new TimeSpan(0, 20, 0));
        public Countdown CountDownTimer { get => countDownTimer; set { countDownTimer = value; OnPropertyChanged(); } }

        private void StartTimer()
        {
            CountDownTimer.Start();
            TimerStartBtn.Content = "Cancel Timer";
        }
        #endregion

        private void Datagrid_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;

            dataGrid.ItemContainerGenerator.StatusChanged += (sender, e) => ItemContainerGenerator_StatusChanged(sender, dataGrid);
        }

        private void ItemContainerGenerator_StatusChanged(object sender, DataGrid dataGrid)
        {
            ItemContainerGenerator icg = (ItemContainerGenerator)sender;

            if (icg.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (DataGridColumn col in dataGrid.Columns)
                {
                    DataGridLength width = col.Width;
                    col.Width = 0;
                    col.Width = width;
                }
            }
        }

        private void Datagrid_Unloaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;

            dataGrid.ItemContainerGenerator.StatusChanged -= (sender, e) => ItemContainerGenerator_StatusChanged(sender, dataGrid);
        }
    }
}