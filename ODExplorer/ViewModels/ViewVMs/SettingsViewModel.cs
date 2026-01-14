using ODExplorer.Database;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODExplorer.ViewModels.ModelVMs;
using ODUtils.Commands;
using ODUtils.Database.Interfaces;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class SettingsViewModel : OdViewModelBase
    {
        public SettingsViewModel(SettingsStore settingsStore,
                         IOdToolsDatabaseProvider databaseProvider,
                         NavigationViewModel navigationView,
                         JournalParserStore parserStore)
        {
            this.settingsStore = settingsStore;
            this.databaseProvider = databaseProvider;
            this.navigationView = navigationView;
            this.parserStore = parserStore;

            OpenPayPal = new RelayCommand(OnOpenPayPal);
            OpenEdsm = new RelayCommand(OnOpenEdsm);
            OpenSpansh = new RelayCommand(OnOpenSpansh);
            OpenGitHub = new RelayCommand(OnOpenGitHub);
            OpenEdAstro = new RelayCommand(OnOpenEdAstro);

            ToggleCommanderHidden = new RelayCommand(OnToggleCommanderHidden, (_) => IsLoaded && SelectedCommander != null);
            ResetLastReadFile = new RelayCommand(OnResetLastFile, (_) => IsLoaded && SelectedCommander != null);
            SaveCommanderChanges = new AsyncRelayCommand(OnSaveCommanderChanges, () => IsLoaded && SelectedCommander != null);
            ResetDataBaseCommand = new RelayCommand(OnResetDataBase, (_) => IsLoaded);
            DeleteCommander = new AsyncRelayCommand(OnDeleteCommander, () => SelectedCommander?.Id != settingsStore.SelectedCommanderID);

            SystemGridSettingsClone = settingsStore.SystemGridSetting.Clone();
        }

        public static SettingsViewModel CreateViewModel(
            SettingsStore settingsStore,
            IOdToolsDatabaseProvider databaseProvider,
            NavigationViewModel navigationView,
            JournalParserStore parserStore)
        {
            var vm = new SettingsViewModel(settingsStore, databaseProvider, navigationView, parserStore);
            _ = vm.Initialise();
            return vm;
        }

        private readonly SettingsStore settingsStore;
        private readonly IOdToolsDatabaseProvider databaseProvider;
        private readonly NavigationViewModel navigationView;
        private readonly JournalParserStore parserStore;

        private readonly SystemGridSettings SystemGridSettingsClone;

        private async Task Initialise()
        {
            await LoadCommanders();
        }

        public override void Dispose()
        {
            if (SystemGridSettingsClone.Equals(settingsStore.SystemGridSetting) == false)
            {
                settingsStore.OnSystemGridSettingsUpdated();
            }
            base.Dispose();
        }

        private ObservableCollection<JournalCommaderViewModel> journalCommanderViewModelCollection = [];
        public ObservableCollection<JournalCommaderViewModel> JournalCommanderViews
        {
            get => journalCommanderViewModelCollection;
            set => journalCommanderViewModelCollection = value;
        }

        private JournalCommaderViewModel? selectedCommander;
        public JournalCommaderViewModel? SelectedCommander
        {
            get => selectedCommander;
            set
            {
                selectedCommander = value;
                OnPropertyChanged(nameof(SelectedCommander));
            }
        }

        private ObservableCollection<IgnoredSystemsViewModel> ignoredSystems = [];

        public ObservableCollection<IgnoredSystemsViewModel> IgnoredSystems
        {
            get => ignoredSystems;
            set => ignoredSystems = value;
        }

        private IgnoredSystemsViewModel? selectedIgnoredSystem;
        public IgnoredSystemsViewModel? SelectedIgnoreSystem
        {
            get => selectedIgnoredSystem;
            set
            {
                selectedIgnoredSystem = value;
                OnPropertyChanged(nameof(SelectedIgnoreSystem));
            }
        }

        private string directoryScanText = string.Empty;
        public string DirectoryScanText
        {
            get => directoryScanText;
            set
            {
                directoryScanText = value;
                OnPropertyChanged(nameof(DirectoryScanText));
            }
        }

        private string fileReadingText = string.Empty;
        public string FileReadingText
        {
            get => fileReadingText;
            set
            {
                fileReadingText = value;
                OnPropertyChanged(nameof(FileReadingText));
            }
        }

        private bool isLoaded;
        public bool IsLoaded
        {
            get => isLoaded;
            set
            {
                isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }

        public JournalLogAge CartoAge
        {
            get => settingsStore.JournalAge;
            set
            {
                if (value == settingsStore.JournalAge)
                    return;

                settingsStore.JournalAge = value;
                OnPropertyChanged(nameof(CartoAge));
                navigationView.LoadingViewCommand.Execute(this);
            }
        }
        private Visibility scanningWindowVisibility = Visibility.Visible;
        public Visibility ScanningWindowVisibility { get => scanningWindowVisibility; set { scanningWindowVisibility = value; OnPropertyChanged(nameof(ScanningWindowVisibility)); } }

        public BodySortCategory BodySortCategory
        {
            get => settingsStore.SystemGridSetting.BodySortingOptions;
            set => settingsStore.SystemGridSetting.BodySortingOptions = value;
        }

        public ListSortDirection SortDirection
        {
            get => settingsStore.SystemGridSetting.SortDirection;
            set => settingsStore.SystemGridSetting.SortDirection = value;
        }

        public bool ExcludeStarsFromRoute
        {
            get => settingsStore.SystemGridSetting.ExcludeStarsFromSorting;
            set => settingsStore.SystemGridSetting.ExcludeStarsFromSorting = value;
        }

        public bool IgnoreNonBodies
        {
            get => settingsStore.SystemGridSetting.IgnoreNonBodies;
            set => settingsStore.SystemGridSetting.IgnoreNonBodies = value;
        }

        public bool ShowBodyHeaders
        {
            get => settingsStore.SystemGridSetting.ShowBodyHeaders;
            set => settingsStore.SystemGridSetting.ShowBodyHeaders = value;
        }

        public bool HideUnconfirmedBios
        {
            get => settingsStore.SystemGridSetting.FilterUnconfirmedBios;
            set => settingsStore.SystemGridSetting.FilterUnconfirmedBios = value;
        }

        public bool ShowBodyIcon
        {
            get => settingsStore.SystemGridSetting.ShowBodyIcon;
            set => settingsStore.SystemGridSetting.ShowBodyIcon = value;
        }

        public bool ShowBodyId
        {
            get => settingsStore.SystemGridSetting.ShowBodyId;
            set => settingsStore.SystemGridSetting.ShowBodyId = value;
        }

        public BodyInfoIconDisplay IconDisplay
        {
            get => settingsStore.SystemGridSetting.InfoDisplayOptions;
            set => settingsStore.SystemGridSetting.InfoDisplayOptions = value;
        }

        public Temperature TemperatureUnit
        {
            get => settingsStore.SystemGridSetting.TemperatureDisplay;
            set => settingsStore.SystemGridSetting.TemperatureDisplay = value;
        }

        public Pressure PressureUnit
        {
            get => settingsStore.SystemGridSetting.PressureUnit;
            set => settingsStore.SystemGridSetting.PressureUnit = value;
        }

        public Distance DistanceUnit
        {
            get => settingsStore.SystemGridSetting.DistanceUnit;
            set => settingsStore.SystemGridSetting.DistanceUnit = value;
        }

        public int MinExoValue
        {
            get => settingsStore.SystemGridSetting.MinExoValue / 1_000_000;
            set => settingsStore.SystemGridSetting.MinExoValue = value * 1_000_000;
        }

        public bool MinimiseToTray
        {
            get => settingsStore.MinimiseToTray;
            set
            {
                settingsStore.SetMinimiseToTray(value);
                OnPropertyChanged();
            }
        }
        #region Commands
        public ICommand OpenPayPal { get; }
        public ICommand ToggleCommanderHidden { get; }
        public ICommand ResetLastReadFile { get; }
        public ICommand SaveCommanderChanges { get; }
        public ICommand ResetDataBaseCommand { get; }
        public ICommand OpenGitHub { get; }
        public ICommand OpenEdsm { get; }
        public ICommand OpenSpansh { get; }
        public ICommand OpenEdAstro { get; }
        public ICommand DeleteCommander { get; }

        private async Task OnSaveCommanderChanges()
        {
            if (SelectedCommander == null)
                return;

            DirectoryScanText = FileReadingText = string.Empty;
            ScanningWindowVisibility = Visibility.Visible;
            foreach (var commander in JournalCommanderViews)
            {
                databaseProvider.AddCommander(new(commander.Id, commander.Name, commander.JournalPath, commander.LastFile, commander.IsHidden));
            }
            await parserStore.UpdateCommanders();

            var currentCommanders = await databaseProvider.GetAllJournalCommanders(true);

            var selectedCommander = currentCommanders.FirstOrDefault(x => x.Id == SelectedCommander.Id);

            if (selectedCommander is null)
            {
                return;
            }

            ScanningWindowVisibility = Visibility.Collapsed;
        }

        private async Task OnDeleteCommander()
        {
            if (SelectedCommander is null)
                return;

            if(databaseProvider is OdExplorerDatabaseProvider provider)
            {
                ScanningWindowVisibility = Visibility.Visible;

                DirectoryScanText = $"DELETING COMMANER {SelectedCommander.Name}";

                await Task.Factory.StartNew(() => provider.DeleteCommander(SelectedCommander.Id)).ConfigureAwait(true);

                await parserStore.UpdateCommanders();

                await LoadCommanders();
            }
        }

        internal void OnSetNewDir(string path)
        {
            if (SelectedCommander != null)
            {
                SelectedCommander.JournalPath = path;
                SelectedCommander.LastFile = string.Empty;
            }
        }

        private void OnOpenPayPal(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&no_recurring=0&item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&currency_code=GBP");
            //https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&no_recurring=0&item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&currency_code=GBP
            //https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&;no_recurring=0&;item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&;currency_code=GBP
            //https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&no_recurring=0&item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&currency_code=GBP
        }

        private void OnOpenEdsm(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://www.edsm.net/");
        }

        private void OnOpenSpansh(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://www.spansh.co.uk/plotter");
        }

        private void OnOpenGitHub(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://github.com/WarmedxMints/OD-Explorer");
        }

        private void OnOpenEdAstro(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://edastro.com/");
        }

        private void OnToggleCommanderHidden(object? obj)
        {
            if (SelectedCommander != null)
                SelectedCommander.IsHidden = !SelectedCommander.IsHidden;
        }
        private void OnResetLastFile(object? obj)
        {
            if (SelectedCommander != null)
                SelectedCommander.LastFile = string.Empty;
        }

        private void OnResetDataBase(object? obj)
        {

            var args = new MessageBoxEventArgsAsync("Reset Database",
                                               "This will reset all Commander data and scan the default directory\n\nAre you sure?",
                                               MessageBoxButton.YesNo,
                                               ResetDatabaseActual);

            navigationView.InvokeMessageBox(args);
        }

        private async Task ResetDatabaseActual()
        {
            if (databaseProvider is OdExplorerDatabaseProvider provider)
            {
                ScanningWindowVisibility = Visibility.Visible;
                DirectoryScanText = $"Resetting Database";
                JournalCommanderViews.Clear();
                SelectedCommander = null;
                OnPropertyChanged(nameof(SelectedCommander));
                OnPropertyChanged(nameof(JournalCommanderViews));
                await Task.Run(async () => await parserStore.ResetDataBase(provider));
                settingsStore.SelectedCommanderID = 0;
                DirectoryScanText = $"Scanning Default Directory";
                navigationView.LoadingViewCommand.Execute(null);
            }
        }

        public void OnScanNewDirectory(string path)
        {
            parserStore.ReadNewDirectory(path);
            navigationView.LoadingViewCommand.Execute(null);
        }
        #endregion

        private async Task LoadCommanders()
        {
            var commanders = await databaseProvider.GetAllJournalCommanders(true);

            var vms = commanders.Select(x => new JournalCommaderViewModel(x));

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                JournalCommanderViews.Clear();

                foreach (var commander in vms)
                {
                    JournalCommanderViews.Add(commander);
                }

                SelectedCommander = JournalCommanderViews.FirstOrDefault(x => x.Id == settingsStore.SelectedCommanderID);
                OnPropertyChanged(nameof(SelectedCommander));
                OnPropertyChanged(nameof(JournalCommanderViews));
                ScanningWindowVisibility = Visibility.Collapsed;
                IsLoaded = true;
            });
        }

        internal void OnExoMinValueChanged()
        {
            settingsStore.OnExoMinValueChanged();
        }
    }
}
