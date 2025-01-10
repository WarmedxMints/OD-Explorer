using EliteJournalReader;
using ODExplorer.Extensions;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODExplorer.ViewModels.ModelVMs;
using ODUtils.Commands;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Exobiology;
using ODUtils.Extensions;
using ODUtils.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class OrganicViewModel : OdViewModelBase
    {
        public OrganicViewModel(OrganicCheckListDataStore organicCheckListDataStore, SettingsStore settingsStore, JournalParserStore parserStore, ExplorationDataStore explorationDataStore, ExoData exoData)
        {
            this.checkListDataStore = organicCheckListDataStore;
            this.settingsStore = settingsStore;
            this.parserStore = parserStore;
            this.explorationDataStore = explorationDataStore;

            checkListDataStore.OnOrganicScanDetailsUpdated += CheckListDataStore_OnOrganicScanDetailsUpdated;
            checkListDataStore.OnSpeciesUpdated += CheckListDataStore_OnSpeciesUpdated;
            explorationDataStore.OnBioDataUpdated += ExplorationDataStore_OnBioDataUpdated;
            explorationDataStore.OnBioDataSold += ExplorationDataStore_OnBioDataSold;
            explorationDataStore.OnBioDataLost += ExplorationDataStore_OnBioDataLost;

            SwitchToCheckList = new RelayCommand(OnSwitchToCheckList, (_) => CurrentState != ExoBiologyViewState.CheckList);
            SwitchToUnSoldList = new RelayCommand(OnSwitchToUnsoldList, (_) => CurrentState != ExoBiologyViewState.UnSoldList);
            SwitchToSoldList = new RelayCommand(OnSwitchToSoldList, (_) => CurrentState != ExoBiologyViewState.Sold);
            SwitchToLostList = new RelayCommand(OnSwitchToLostList, (_) => CurrentState != ExoBiologyViewState.Lost);

            SelectedRegionText = $"{SelectedRegion.GetEnumDescription()} Selected";
            SelectedRegionList = [SelectedRegion];

            BuildUnsoldList();
            BuildSoldList();
            BuildLostList();
            CountBios();
        }

        public override void Dispose()
        {
            checkListDataStore.OnOrganicScanDetailsUpdated -= CheckListDataStore_OnOrganicScanDetailsUpdated;
            checkListDataStore.OnSpeciesUpdated -= CheckListDataStore_OnSpeciesUpdated;
            explorationDataStore.OnBioDataUpdated -= ExplorationDataStore_OnBioDataUpdated;
            explorationDataStore.OnBioDataSold -= ExplorationDataStore_OnBioDataSold;
            explorationDataStore.OnBioDataLost -= ExplorationDataStore_OnBioDataLost;
        }

        private readonly OrganicCheckListDataStore checkListDataStore;
        private readonly SettingsStore settingsStore;
        private readonly JournalParserStore parserStore;
        private readonly ExplorationDataStore explorationDataStore;

        private ObservableCollection<OrganicScanItemViewModel> unSold = [];
        public ObservableCollection<OrganicScanItemViewModel> Unsold
        {
            get => unSold;
            set
            {
                unSold = value;
                OnPropertyChanged(nameof(Unsold));
            }
        }

        private ObservableCollection<OrganicScanItemViewModel> sold = [];
        public ObservableCollection<OrganicScanItemViewModel> Sold
        {
            get => sold;
            set
            {
                sold = value;
                OnPropertyChanged(nameof(Sold));
            }
        }

        private ObservableCollection<OrganicScanItemViewModel> lost = [];
        public ObservableCollection<OrganicScanItemViewModel> Lost
        {
            get => lost;
            set
            {
                lost = value;
                OnPropertyChanged(nameof(Lost));
            }
        }

        private OrganicCheckListItemViewModel? selectedItem;
        public OrganicCheckListItemViewModel? SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));

                if (selectedItem != null)
                {
                    SelectedRegionList = ExoData.SpeciesRegions[selectedItem.CodexValue];

                    var names = ExoData.GetNamesFromSpecies(selectedItem.CodexValue);

                    SelectedRegionText = names is not null ? $"{names.Genus} {names.Species} Is Found In Highlighted Regions" : $"{selectedItem.Name} Is Found In Highlighted Regions";
                    OnPropertyChanged(nameof(SelectedRegionList));
                    OnPropertyChanged(nameof(SelectedRegionText));
                    return;
                }

                SelectedRegionList = [SelectedRegion];
                SelectedRegionText = $"{SelectedRegion.GetEnumDescription()} Selected";
                OnPropertyChanged(nameof(SelectedRegionList));
                OnPropertyChanged(nameof(SelectedRegionText));
            }
        }

        public string SelectedRegionText { get; private set; } = string.Empty;
        public string SelectedRegionSpeciesCount { get; private set; } = string.Empty;
        public string SelectedRegionVariantCount { get; private set; } = string.Empty;

        public IReadOnlyList<OrganicCheckListItemViewModel> Aleoida => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Aleoids_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Bacterium => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Bacterial_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Cactoida => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Cactoid_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Clypeus => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Clypeus_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Concha => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Conchas_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Electricae => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Electricae_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Fonticulua => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Fonticulus_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Fumerola => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Fumerolas_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Fungoida => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Fungoids_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Osseus => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Osseus_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Recepta => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Recepta_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Frutexa => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Shrubs_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Stratum => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Stratum_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Tubus => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Tubus_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Tussock => CheckInRegion(checkListDataStore.OrganicScanItems["$Codex_Ent_Tussocks_Genus_Name;"]);
        public IReadOnlyList<OrganicCheckListItemViewModel> Other => CheckInRegion(checkListDataStore.OrganicScanItems["Other"]);

        public GalacticRegions SelectedRegion
        {
            get => settingsStore.ExoCheckListRegion;
            set
            {
                if (settingsStore.ExoCheckListRegion == value)
                    return;
                settingsStore.ExoCheckListRegion = value;
                SelectedRegionList = [value];
                SelectedRegionText = $"{SelectedRegion.GetEnumDescription()} Selected";
                OnPropertyChanged(nameof(SelectedRegionList));
                OnPropertyChanged(nameof(SelectedRegionText));
                CheckListDataStore_OnOrganicScanDetailsUpdated(null, EventArgs.Empty);
            }
        }

        public List<GalacticRegions> SelectedRegionList { get; private set; }

        public ExoBiologyViewState CurrentState
        {
            get
            {
                if (parserStore.IsLive == false)
                {
                    return ExoBiologyViewState.None;
                }
                return settingsStore.BiologyViewState;
            }
            set
            {
                settingsStore.BiologyViewState = value;
                OnPropertyChanged(nameof(CurrentState));
            }
        }

        public Visibility OthersVisibility { get => Other.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }

        public ICommand SwitchToCheckList { get; }
        public ICommand SwitchToUnSoldList { get; }
        public ICommand SwitchToSoldList { get; }
        public ICommand SwitchToLostList { get; }

        private void OnSwitchToUnsoldList(object? obj)
        {
            CurrentState = ExoBiologyViewState.UnSoldList;
        }

        private void OnSwitchToSoldList(object? obj)
        {
            CurrentState = ExoBiologyViewState.Sold;
        }

        private void OnSwitchToCheckList(object? obj)
        {
            CurrentState = ExoBiologyViewState.CheckList;
        }

        private void OnSwitchToLostList(object? obj)
        {
            CurrentState = ExoBiologyViewState.Lost;
        }

        private List<OrganicCheckListItemViewModel> CheckInRegion(List<OrganicChecklistItem> items)
        {
            if (items is null || items.Count == 0)
                return [];

            if (SelectedRegion == GalacticRegions.Unknown)
            {
                var ret = items.Select(x => new OrganicCheckListItemViewModel()
                {
                    CodexValue = x.SpeciesCodex,
                    Name = x.Name,
                    State = x.Region.Max(x => x.Value),
                    Variants = [.. x.Variants.Values
                                .Where(x => x.Any(x => x.State > OrganicScanState.Unavailable))
                                .SelectMany(x => x)
                                .Where(x => x.State > OrganicScanState.Unavailable)
                                .GroupBy(x => x.LocalName)
                                .Select(x => x.MaxBy(y => y.State))
                                .Select(x => new OrganicCheckListVariantViewModel(x))
                                .OrderBy(x => x.LocalName)]
                }).ToList();

                return ret;
            }

            if (items.Any(x => x.Region.ContainsKey(SelectedRegion)))
            {
                return items.Select(x => new OrganicCheckListItemViewModel()
                {
                    CodexValue = x.SpeciesCodex,
                    Name = x.Name,
                    State = x.Region[SelectedRegion],
                    Variants = x.Variants.TryGetValue(SelectedRegion, out var list) ?
                                    [.. list.Select(x => new OrganicCheckListVariantViewModel(x)).OrderBy(x => x.LocalName)] : []
                }).ToList();
            }
            return [];
        }

        private void CheckListDataStore_OnSpeciesUpdated(object? sender, string e)
        {
            FireUpdatePropertyChanged(e);
        }

        private void CheckListDataStore_OnOrganicScanDetailsUpdated(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Aleoida));
            OnPropertyChanged(nameof(Bacterium));
            OnPropertyChanged(nameof(Cactoida));
            OnPropertyChanged(nameof(Clypeus));
            OnPropertyChanged(nameof(Concha));
            OnPropertyChanged(nameof(Electricae));
            OnPropertyChanged(nameof(Fonticulua));
            OnPropertyChanged(nameof(Fumerola));
            OnPropertyChanged(nameof(Fungoida));
            OnPropertyChanged(nameof(Osseus));
            OnPropertyChanged(nameof(Recepta));
            OnPropertyChanged(nameof(Frutexa));
            OnPropertyChanged(nameof(Stratum));
            OnPropertyChanged(nameof(Tubus));
            OnPropertyChanged(nameof(Tussock));
            OnPropertyChanged(nameof(Other));
            OnPropertyChanged(nameof(OthersVisibility));

            CountBios();
        }

        private void CountBios()
        {
            var species = 0;
            var speciesFound = 0;
            var variants = 0;
            var variantsFound = 0;

            var allSpecies = new List<OrganicCheckListItemViewModel>();
            allSpecies.AddRange(Aleoida);
            allSpecies.AddRange(Bacterium);
            allSpecies.AddRange(Cactoida);
            allSpecies.AddRange(Clypeus);
            allSpecies.AddRange(Concha);
            allSpecies.AddRange(Electricae);
            allSpecies.AddRange(Fonticulua);
            allSpecies.AddRange(Fumerola);
            allSpecies.AddRange(Fungoida);
            allSpecies.AddRange(Osseus);
            allSpecies.AddRange(Fumerola);
            allSpecies.AddRange(Recepta);
            allSpecies.AddRange(Frutexa);
            allSpecies.AddRange(Stratum);
            allSpecies.AddRange(Tubus);
            allSpecies.AddRange(Tussock);
            allSpecies.AddRange(Other);

            foreach (var specie in allSpecies)
            {
                if (specie.State == OrganicScanState.Unavailable)
                    continue;

                species++;

                if (specie.State >= OrganicScanState.Analysed)
                    speciesFound++;
                variants += specie.Variants.Count;

                variantsFound += specie.Variants.Where(x => x.StageValue >= OrganicScanState.Analysed).Count();
            }

            SelectedRegionSpeciesCount = $"{speciesFound} / {species}";
            SelectedRegionVariantCount = $"{variantsFound} / {variants}";

            OnPropertyChanged(nameof(SelectedRegionSpeciesCount));
            OnPropertyChanged(nameof(SelectedRegionVariantCount));
        }

        private void FireUpdatePropertyChanged(string key)
        {
            switch (key)
            {
                case "$Codex_Ent_Aleoids_Genus_Name;":
                    UpdateSpeciesCounts(Aleoida);
                    OnPropertyChanged(nameof(Aleoida));
                    break;
                case "$Codex_Ent_Bacterial_Genus_Name;":
                    UpdateSpeciesCounts(Bacterium);
                    OnPropertyChanged(nameof(Bacterium));
                    break;
                case "$Codex_Ent_Cactoid_Genus_Name;":
                    UpdateSpeciesCounts(Cactoida);
                    OnPropertyChanged(nameof(Cactoida));
                    break;
                case "$Codex_Ent_Clypeus_Genus_Name;":
                    UpdateSpeciesCounts(Clypeus);
                    OnPropertyChanged(nameof(Clypeus));
                    break;
                case "$Codex_Ent_Conchas_Genus_Name;":
                    UpdateSpeciesCounts(Concha);
                    OnPropertyChanged(nameof(Concha));
                    break;
                case "$Codex_Ent_Electricae_Genus_Name":
                    UpdateSpeciesCounts(Electricae);
                    OnPropertyChanged(nameof(Electricae));
                    break;
                case "$Codex_Ent_Fonticulus_Genus_Name":
                    UpdateSpeciesCounts(Fonticulua);
                    OnPropertyChanged(nameof(Fonticulua));
                    break;
                case "$Codex_Ent_Fumerolas_Genus_Name;":
                    UpdateSpeciesCounts(Fumerola);
                    OnPropertyChanged(nameof(Fumerola));
                    break;
                case "$Codex_Ent_Fungoids_Genus_Name;":
                    UpdateSpeciesCounts(Fungoida);
                    OnPropertyChanged(nameof(Fungoida));
                    break;
                case "$Codex_Ent_Osseus_Genus_Name;":
                    UpdateSpeciesCounts(Osseus);
                    OnPropertyChanged(nameof(Osseus));
                    break;
                case "$Codex_Ent_Recepta_Genus_Name;":
                    UpdateSpeciesCounts(Recepta);
                    OnPropertyChanged(nameof(Recepta));
                    break;
                case "$Codex_Ent_Shrubs_Genus_Name;":
                    UpdateSpeciesCounts(Frutexa);
                    OnPropertyChanged(nameof(Frutexa));
                    break;
                case "$Codex_Ent_Stratum_Genus_Name;":
                    UpdateSpeciesCounts(Stratum);
                    OnPropertyChanged(nameof(Stratum));
                    break;
                case "$Codex_Ent_Tubus_Genus_Name;":
                    UpdateSpeciesCounts(Tubus);
                    OnPropertyChanged(nameof(Tubus));
                    break;
                case "$Codex_Ent_Tussocks_Genus_Name;":
                    UpdateSpeciesCounts(Tussock);
                    OnPropertyChanged(nameof(Tussock));
                    break;
                case "Other":
                    UpdateSpeciesCounts(Other);
                    OnPropertyChanged(nameof(Other));
                    break;
            }

            CountBios();
        }

        private static void UpdateSpeciesCounts(IReadOnlyList<OrganicCheckListItemViewModel> list)
        {
            foreach(var item in list)
            {
                item.UpdateCounts();
            }
        }
        private void ExplorationDataStore_OnBioDataLost(object? sender, EventArgs e)
        {
            BuildLostList();
        }

        private void ExplorationDataStore_OnBioDataSold(object? sender, EventArgs e)
        {
            BuildSoldList();
        }

        private void ExplorationDataStore_OnBioDataUpdated(object? sender, OrganicScanItem e)
        {
            BuildUnsoldList();
        }

        private void BuildUnsoldList()
        {
            Unsold.ClearCollection();

            var modelList = new List<OrganicScanItemViewModel>();

            foreach (var body in explorationDataStore.OrganicScanItems)
            {
                if (body.OrganicScanItems is null)
                    continue;

                modelList.AddRange(body.OrganicScanItems.Where(x => x.ScanStage == OrganicScanStage.Analyse && x.DataState == DataState.Unsold).Select(x => new OrganicScanItemViewModel(x)));
            }
            modelList.Sort((x, y) => string.Compare(x.BodyName, y.BodyName, StringComparison.Ordinal));
            Unsold.AddRangeToCollection(modelList);
            OnPropertyChanged(nameof(Unsold));
        }

        private void BuildSoldList()
        {
            Sold.ClearCollection();

            var modelList = new List<OrganicScanItemViewModel>();

            foreach (var body in explorationDataStore.OrganicScanItems)
            {
                if (body.OrganicScanItems is null)
                    continue;

                modelList.AddRange(body.OrganicScanItems.Where(x => x.DataState == DataState.Sold).Select(x => new OrganicScanItemViewModel(x)));
            }

            Sold.AddRangeToCollection(modelList.OrderBy(x => x.EnglishName, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.BodyName, StringComparer.OrdinalIgnoreCase));
            OnPropertyChanged(nameof(Sold));
        }

        private void BuildLostList()
        {
            Lost.ClearCollection();

            var modelList = new List<OrganicScanItemViewModel>();

            foreach (var body in explorationDataStore.OrganicScanItems)
            {
                if (body.OrganicScanItems is null)
                    continue;

                modelList.AddRange(body.OrganicScanItems.Where(x => x.DataState == DataState.Lost).Select(x => new OrganicScanItemViewModel(x)));
            }

            Lost.AddRangeToCollection(modelList.OrderBy(x => x.EnglishName, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.BodyName, StringComparer.OrdinalIgnoreCase));
            OnPropertyChanged(nameof(Lost));
        }
    }
}
