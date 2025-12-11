using ODExplorer.Database;
using ODExplorer.Extensions;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODExplorer.ViewModels.ModelVMs;
using ODUtils.APis;
using ODUtils.Commands;
using ODUtils.Database.Interfaces;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class CartoDetailsViewModel : OdViewModelBase
    {
        private readonly ExplorationDataStore explorationData;
        private readonly SettingsStore settingsStore;
        private readonly EdsmApiService edsmApi;
        private readonly IOdToolsDatabaseProvider databaseProvider;
        private readonly JournalParserStore parserStore;
        private readonly NotificationStore notificationStore;
        private readonly ObservableCollection<StarSystemViewModel> unsoldSystems = [];
        private readonly ObservableCollection<StarSystemViewModel> soldSystems = [];
        private readonly ObservableCollection<StarSystemViewModel> lostSystems = [];
        private ObservableCollection<IgnoredSystemsViewModel> ignoredSystems = [];

        public ObservableCollection<IgnoredSystemsViewModel> IgnoredSystems
        {
            get => ignoredSystems;
            set => ignoredSystems = value;
        }
        public ObservableCollection<StarSystemViewModel> UnsoldSystems { get => unsoldSystems; }
        public ObservableCollection<StarSystemViewModel> SoldSystems { get => soldSystems; }
        public ObservableCollection<StarSystemViewModel> LostSystems { get => lostSystems; }

        private StarSystemViewModel? selectedSystem;
        public StarSystemViewModel? SelectedSystem
        {
            get => selectedSystem;
            set
            {
                selectedSystem = value;
                OnPropertyChanged(nameof(SelectedSystem));
            }
        }
        public string? SelectedCommanderName => parserStore.JournalCommanders.FirstOrDefault(x => x.Id == settingsStore.SelectedCommanderID)?.Name;

        public ICommand OpenEDSM { get; }
        public ICommand OpenSpansh { get; }
        public ICommand CopyToClipboard { get; }
        public ICommand AddToIgnoreList { get; }
        public ICommand SwitchToUnsold { get; }
        public ICommand SwitchToSold { get; }
        public ICommand SwitchToLost { get; }
        public ICommand SwitchToIgnored { get; }
        public ICommand ToggleRestoreCommand { get; }
        public ICommand SaveRestoreCommand { get; }

        public CartoDetailsViewState CurrentState
        {
            get => settingsStore.CartoDetailsViewState;
            set { settingsStore.CartoDetailsViewState = value; OnPropertyChanged(); }
        }

        public CartoDetailsViewModel(ExplorationDataStore explorationData,
                                     SettingsStore settingsStore,
                                     EdsmApiService edsmApi,
                                     IOdToolsDatabaseProvider databaseProvider,
                                     JournalParserStore parserStore,
                                     NotificationStore notificationStore)
        {
            this.explorationData = explorationData;
            this.settingsStore = settingsStore;
            this.edsmApi = edsmApi;
            this.databaseProvider = databaseProvider;
            this.parserStore = parserStore;
            this.notificationStore = notificationStore;

            OpenEDSM = new AsyncRelayCommand<StarSystemViewModel?>(OnOpenEdsm);
            OpenSpansh = new RelayCommand<StarSystemViewModel?>(OnOpenSpansh);
            AddToIgnoreList = new RelayCommand<StarSystemViewModel?>(OnAddToIgnoreList);
            SwitchToUnsold = new RelayCommand(OnSwitchToUnsold, (_) => CurrentState != CartoDetailsViewState.Unsold);
            SwitchToSold = new RelayCommand(OnSwitchToSold, (_) => CurrentState != CartoDetailsViewState.Sold);
            SwitchToLost = new RelayCommand(OnSwitchToLost, (_) => CurrentState != CartoDetailsViewState.Lost);
            SwitchToIgnored = new RelayCommand(OnSwitchToIgnored, (_) => CurrentState != CartoDetailsViewState.Ignored);
            ToggleRestoreCommand = new RelayCommand<IgnoredSystemsViewModel>(OnToggleRestore);
            SaveRestoreCommand = new RelayCommand(OnSaveIgnoredSystems, (_) => IgnoredSystems.Any(x => x.Restore));
            CopyToClipboard = new RelayCommand<string>(OnCopyToClipboard);

            explorationData.OnBodyUpdated += ExplorationData_OnBodyUpdated;
            explorationData.OnCartoDataSold += ExplorationData_OnCartoDataSold;
            explorationData.OnCartoDataLost += ExplorationData_OnCartoDataLost;

            BuildUnsoldSystems();
            BuildSoldSystems();
            BuildLostSystems();
            BuildIgnoreSystems();
        }

        public override void Dispose()
        {
            explorationData.OnBodyUpdated -= ExplorationData_OnBodyUpdated;
            explorationData.OnCartoDataSold -= ExplorationData_OnCartoDataSold;
            explorationData.OnCartoDataLost -= ExplorationData_OnCartoDataLost;
        }

        private void ExplorationData_OnCartoDataLost(object? sender, EventArgs e)
        {
            BuildLostSystems();
        }

        private void ExplorationData_OnCartoDataSold(object? sender, EventArgs e)
        {
            BuildSoldSystems();
        }

        private void ExplorationData_OnBodyUpdated(object? sender, SystemBody e)
        {
            BuildUnsoldSystems();
        }

        private void OnSwitchToIgnored(object? obj)
        {
            CurrentState = CartoDetailsViewState.Ignored;
        }

        private void OnSwitchToLost(object? obj)
        {
            CurrentState = CartoDetailsViewState.Lost;
        }

        private void OnSwitchToSold(object? obj)
        {
            CurrentState = CartoDetailsViewState.Sold;
        }

        private void OnSwitchToUnsold(object? obj)
        {
            CurrentState = CartoDetailsViewState.Unsold;
        }

        private void OnCopyToClipboard(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            notificationStore.CopyToClipBoard(value);
        }
        private void OnSaveIgnoredSystems(object? obj)
        {
            var systemsToSave = IgnoredSystems.Where(x => x.Restore);

            if (systemsToSave.Any() == false)
                return;

            if (databaseProvider is OdExplorerDatabaseProvider provider)
            {
                foreach (var system in systemsToSave)
                {
                    provider.RemoveIgnoreSystem(system.Address, system.CmdrId);
                }

                explorationData.PopulateIgnoredSystems(settingsStore.SelectedCommanderID);
            }

            BuildIgnoreSystems();
            BuildUnsoldSystems();
        }

        private void OnToggleRestore(IgnoredSystemsViewModel model)
        {
            if (model == null)
                return;

            model.Restore = !model.Restore;
            OnPropertyChanged(nameof(IgnoredSystems));
        }


        private void OnAddToIgnoreList(StarSystemViewModel? model)
        {
            if (model != null && databaseProvider is OdExplorerDatabaseProvider provider)
            {
                provider.AddIgnoreSystem(model.Address, model.Name, settingsStore.SelectedCommanderID);
                explorationData.PopulateIgnoredSystems(settingsStore.SelectedCommanderID);
                BuildUnsoldSystems();
                BuildIgnoreSystems();
            }
        }

        private void OnOpenSpansh(StarSystemViewModel? model)
        {
            if (model == null)
                return;

            ODUtils.Helpers.OperatingSystem.OpenUrl($"https://spansh.co.uk/system/{model.Address}");
        }

        private async Task OnOpenEdsm(StarSystemViewModel? model)
        {
            if (model == null)
                return;

            if (string.IsNullOrEmpty(model.EdsmUrl))
            {
                var ret = await edsmApi.GetSystemUrlAsync(model.Address);

                if (ret != null)
                {
                    ODUtils.Helpers.OperatingSystem.OpenUrl(ret);
                    return;
                }
            }

            ODUtils.Helpers.OperatingSystem.OpenUrl(model.EdsmUrl);
        }

        private void BuildUnsoldSystems()
        {
            UnsoldSystems.ClearCollection();

            var systemsToAdd = explorationData.GetUnsoldCartoSystems().Select(x => StarSystemViewModel.BuildSystemForCartoDetailsView(x, settingsStore, notificationStore, DataState.Unsold));

            var selectedSystem = systemsToAdd.FirstOrDefault(x => x.Address == explorationData.CurrentSystem?.Address);

            selectedSystem ??= systemsToAdd.FirstOrDefault();

            SelectedSystem = selectedSystem;

            UnsoldSystems.AddRangeToCollection(systemsToAdd);
        }

        private void BuildSoldSystems()
        {
            SoldSystems.ClearCollection();

            var systemsToAdd = explorationData.GetSoldCartoSystems().Select(x => StarSystemViewModel.BuildSystemForCartoDetailsView(x, settingsStore, notificationStore, DataState.Sold));

            SoldSystems.AddRangeToCollection(systemsToAdd);
        }

        private void BuildLostSystems()
        {
            LostSystems.ClearCollection();

            var systemsToAdd = explorationData.GetLostCartoSystems().Select(x => StarSystemViewModel.BuildSystemForCartoDetailsView(x, settingsStore, notificationStore, DataState.Lost));

            LostSystems.AddRangeToCollection(systemsToAdd);
        }

        private void BuildIgnoreSystems()
        {
            IgnoredSystems.ClearCollection();

            if (databaseProvider is OdExplorerDatabaseProvider provider)
            {
                var systemsToIgnore = provider.GetIgnoredSystems(settingsStore.SelectedCommanderID);

                IgnoredSystems.AddRangeToCollection(systemsToIgnore.Select(x => new IgnoredSystemsViewModel(x)));
            }

            OnPropertyChanged(nameof(IgnoredSystems));
        }
    }
}
