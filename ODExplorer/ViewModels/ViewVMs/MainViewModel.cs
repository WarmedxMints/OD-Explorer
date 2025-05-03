using ODExplorer.Controls;
using ODExplorer.Extensions;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODExplorer.ViewModels.ModelVMs;
using ODExplorer.Windows;
using ODUtils.Commands;
using ODUtils.Database.Interfaces;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Models;
using ODUtils.ViewModelNavigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class MainViewModel : OdViewModelBase
    {
        #region Constructor
        public MainViewModel(OdNavigationStore navigationStore,
                            NavigationViewModel navigationViewModel,
                            SettingsStore settings,
                            ExplorationDataStore explorationData,
                            JournalParserStore journalParserStore,
                            OrganicCheckListDataStore organicCheckListDataStore,
                            NotificationStore notificationStore,
                            SpanshCsvStore spanshCsvStore,
                            IOdToolsDatabaseProvider databaseProvider)
        {
            _navigationStore = navigationStore;
            _navigationViewModel = navigationViewModel;
            _settings = settings;
            _explorationData = explorationData;
            _journalParserStore = journalParserStore;
            this.organicCheckListDataStore = organicCheckListDataStore;
            this.notificationStore = notificationStore;
            this.spanshCsvStore = spanshCsvStore;
            this.databaseProvider = databaseProvider;

            _settings.OnSystemGridSettingsUpdatedEvent += Settings_OnSystemGridSettingsUpdatedEvent;
            _navigationStore.CurrentViewModelChanged += OnNavigationStore_CurrentViewModelChanged;
            _journalParserStore.OnParserStoreLive += OnJournalParserStore_OnParserStoreLive;
            _journalParserStore.OnCommandersUpdated += OnCommandersUpdated;

            _explorationData.OnCurrentSystemUpdated += OnCurrentSystemUpdated;
            _explorationData.OnRouteUpdated += ExplorationData_OnRouteUpdated;
            _explorationData.OnBodyUpdated += ExplorationData_OnBodyUpdated;
            _explorationData.OnSystemUpdatedFromEDSM += ExplorationData_OnSystemUpdatedFromEDSM;
            _explorationData.OnBioDataUpdated += ExplorationData_OnBioDataUpdated;
            _explorationData.OnAllBodiesDiscovered += ExplorationData_OnAllBodiesDiscovered;
            _explorationData.OnBodyBiosUpdated += ExplorationData_OnBodyBiosUpdated;
            _explorationData.OnFSDJump += ExplorationData_OnFSDJump;
            _explorationData.OnBodyTargeted += ExplorationData_OnBodyTargeted;

            ResetWindowPositionCommand = new RelayCommand(OnResetWindowPos);
            ResetPopOutPositionCommand = new RelayCommand(OnResetPopOutPos);
            NavigateToView = new RelayCommand<ActiveViewModel>(OnNavigateToView, (model) => GetModelType(model) && UiEnabled);

            AdjustUiScale = new RelayCommand(OnAdjustUiScale, (_) => UiEnabled);
            ResetUiScale = new RelayCommand(OnResetUiScale);

            Title = $"OD Explorer v{App.AppVersion}";

            currentSystem = null;
            OrganicSignals.ClearCollection();
            //If the store is already live then no commanders where found
            if (_journalParserStore.IsLive)
            {
                UiEnabled = true;
            }

            OnCurrentSystemUpdated(null, explorationData.CurrentSystem);
            ExplorationData_OnRouteUpdated("Start up", explorationData.Route);
        }
        #endregion

        #region Private Fields
        private readonly OdNavigationStore _navigationStore;
        private readonly NavigationViewModel _navigationViewModel;
        private readonly SettingsStore _settings;
        private readonly ExplorationDataStore _explorationData;
        private readonly JournalParserStore _journalParserStore;
        private readonly OrganicCheckListDataStore organicCheckListDataStore;
        private readonly NotificationStore notificationStore;
        private readonly SpanshCsvStore spanshCsvStore;
        private readonly IOdToolsDatabaseProvider databaseProvider;
        private int currentCommanderId;
        private List<PopOutBase> ActivePopOuts { get; set; } = [];
        #endregion

        #region View Propetires
        public string Title { get; }
        public SettingsStore SettingsStore { get => _settings; }
        public OdViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;
        private bool uiEnabled;
        public bool UiEnabled
        {
            get => uiEnabled;
            private set
            {
                uiEnabled = value;
                OnPropertyChanged(nameof(UiEnabled));
            }
        }
        public double UiScale
        {
            get => SettingsStore.UiScale;
            set
            {
                SettingsStore.UiScale = value;
                OnPropertyChanged(nameof(UiScale));
            }
        }
        #endregion

        #region Commander Properties
        public ObservableCollection<JournalCommaderViewModel> JournalCommanders { get; set; } = [];

        private JournalCommaderViewModel? selectedCommander;
        public JournalCommaderViewModel? SelectedCommander
        {
            get => selectedCommander;
            set
            {
                if (value == selectedCommander)
                    return;
                selectedCommander = value;
                if (UiEnabled && selectedCommander != null && selectedCommander.Id != _settings.SelectedCommanderID)
                {
                    UiEnabled = false;
                    _settings.SelectedCommanderID = selectedCommander.Id;
                    OnNavigateToLoading(null);
                }

                OnPropertyChanged(nameof(SelectedCommander));
            }
        }
        #endregion

        #region CurrentSystem Properties
        private StarSystemViewModel? currentSystem;
        public StarSystemViewModel? CurrentSystem => currentSystem;

        private ObservableCollection<SystemBodyViewModel> organicSignals = [];
        public ObservableCollection<SystemBodyViewModel> OrganicSignals
        {
            get => organicSignals;
            set
            {
                organicSignals = value;
                OnPropertyChanged(nameof(OrganicSignals));
            }
        }

        private ObservableCollection<StarSystemViewModel> route = [];
        public ObservableCollection<StarSystemViewModel> Route
        {
            get => route;
            set
            {
                route = value;
                OnPropertyChanged(nameof(Route));
            }
        }

        private SystemBodyViewModel? selectedBody;
        public SystemBodyViewModel? SelectedBody
        {
            get
            {
                return selectedBody;
            }
            set
            {
                if (selectedBody != value)
                {
                    selectedBody = value;
                    _explorationData.SelectedBodyId = selectedBody?.BodyID ?? 0;
                    OnPropertyChanged(nameof(SelectedBody));
                    OnSelectedBodyUpdated?.Invoke(this, selectedBody);
                }
            }
        }

        private bool inHyperSpace;
        public bool InHyperSpace
        {
            get => inHyperSpace;
            set
            {
                inHyperSpace = value;
                OnPropertyChanged(nameof(InHyperSpace));
            }
        }

        private string hyperSpaceText = string.Empty;
        public string HyperSpaceText
        {
            get => hyperSpaceText;
            set
            {
                hyperSpaceText = value;
                OnPropertyChanged(nameof(HyperSpaceText));
            }
        }

        public string CurrentSystemName
        {
            get
            {
                if (InHyperSpace)
                    return "Hyperspace";
                return _explorationData.CurrentSystemName ?? string.Empty;
            }
        }

        public string CurrentSystemRegion => _explorationData?.CurrentSystemRegion ?? string.Empty;
        public bool FilterUnconfirmedBios => SettingsStore.SystemGridSetting.FilterUnconfirmedBios;
        #endregion

        #region Commands
        public ICommand ResetWindowPositionCommand { get; }
        public ICommand ResetPopOutPositionCommand { get; }
        public ICommand NavigateToView { get; }
        public ICommand AdjustUiScale { get; }
        public ICommand ResetUiScale { get; }
        #endregion

        #region Events & Methods
        public event EventHandler<MessageBoxEventArgsAsync>? OnMessageBoxRequested;
        public event EventHandler? AdjustUiScaleEvent;
        public event EventHandler<StarSystemViewModel?>? OnCurrentSystemUpdatedEvent;
        public event EventHandler<ObservableCollection<StarSystemViewModel>>? OnRouteUpdated;
        public event EventHandler<SystemBodyViewModel>? OnBodyUpdated;
        public event EventHandler<SystemBodyViewModel>? OnBioUpdated;
        public event EventHandler<SystemBodyViewModel?>? OnSelectedBodyUpdated;
        public event EventHandler? WindowReset;
        private void Settings_OnSystemGridSettingsUpdatedEvent(object? sender, EventArgs e)
        {
            if (OrganicSignals.Count != 0)
            {
                foreach (var signal in OrganicSignals)
                {
                    signal.SetAlternationIndexes();
                }
            }
            OnPropertyChanged(nameof(FilterUnconfirmedBios));
        }

        public void InvokeMessageBox(MessageBoxEventArgsAsync e)
        {
            OnMessageBoxRequested?.Invoke(this, e);
        }

        private void OnJournalParserStore_OnParserStoreLive(object? sender, bool e)
        {
            if (e)
            {
                OrganicSignals.ClearCollection();
                Route.ClearCollection();
                currentCommanderId = SettingsStore.SelectedCommanderID;
                OnNavigateToView(_settings.ActiveView);
                UiEnabled = true;
                OnCurrentSystemUpdated(null, _explorationData.CurrentSystem);
                CurrentSystemUpdated();
                OpenActivePopOuts();
                return;
            }
            UiEnabled = false;

            foreach (var popOut in ActivePopOuts)
            {
                popOut.ForceClose();
                SettingsStore.SaveParams(popOut, true, currentCommanderId);
            }
            currentCommanderId = 0;
            ActivePopOuts.Clear();
        }
        #endregion

        #region View Navigation Methods
        private void OnNavigateToView(ActiveViewModel model)
        {
            switch (model)
            {
                case ActiveViewModel.ExoBiology:
                    _settings.ActiveView = ActiveViewModel.ExoBiology;
                    _navigationViewModel.OrganicViewCommand.Execute(null);
                    break;
                case ActiveViewModel.Settings:
                    _settings.ActiveView = ActiveViewModel.Settings;
                    _navigationViewModel.SettingsViewCommand.Execute(null);
                    break;
                case ActiveViewModel.DisplaySettings:
                    _settings.ActiveView = ActiveViewModel.DisplaySettings;
                    _navigationViewModel.DisplaySettingsViewCommand.Execute(null);
                    break;
                case ActiveViewModel.CartoDetails:
                    _settings.ActiveView = ActiveViewModel.CartoDetails;
                    _navigationViewModel.CartoDetailsViewCommand.Execute(null);
                    break;
                case ActiveViewModel.Spansh:
                    _settings.ActiveView = ActiveViewModel.Spansh;
                    _navigationViewModel.SpanshViewCommand.Execute(null);
                    break;
                case ActiveViewModel.EdAstro:
                    _settings.ActiveView = ActiveViewModel.EdAstro;
                    _navigationViewModel.EdAstroViewCommand.Execute(null);
                    break;
                case ActiveViewModel.Carto:
                default:
                    _settings.ActiveView = ActiveViewModel.Carto;
                    _navigationViewModel.CartographicViewCommand.Execute(null);
                    break;
            }
            OnPropertyChanged(nameof(GetModelType));
        }

        private bool GetModelType(ActiveViewModel model)
        {
            return model switch
            {
                ActiveViewModel.Carto => CurrentViewModel is not CartographicViewModel,
                ActiveViewModel.ExoBiology => CurrentViewModel is not OrganicViewModel,
                ActiveViewModel.Settings => CurrentViewModel is not SettingsViewModel,
                ActiveViewModel.DisplaySettings => CurrentViewModel is not DisplaySettingsViewModel,
                ActiveViewModel.CartoDetails => CurrentViewModel is not CartoDetailsViewModel,
                ActiveViewModel.Spansh => CurrentViewModel is not SpanshViewModel,
                ActiveViewModel.EdAstro => CurrentViewModel is not EdAstroViewModel,
                _ => false,
            };
        }

        private void OnNavigateToLoading(object? obj)
        {
            _navigationViewModel.LoadingViewCommand.Execute(obj);
        }

        private void OnNavigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
        #endregion

        #region Command Methods
        private void OnResetUiScale(object? obj)
        {
            UiScale = 1;
        }

        private void OnAdjustUiScale(object? obj)
        {
            AdjustUiScaleEvent?.Invoke(obj, EventArgs.Empty);
        }

        private void OnResetWindowPos(object? obj)
        {
            WindowReset?.Invoke(this, EventArgs.Empty);
            _settings.ResetWindowPosition();
        }

        private void OnResetPopOutPos(object? obj)
        {
//#if DEBUG
//            if(selectedBody != null) 
//                notificationStore.CheckForNotableNotifications(selectedBody.Body);
//            return;
//#else
            foreach (var popOut in ActivePopOuts)
            {
                SettingsStore.ResetWindowPositionActual(popOut.Position, 800, 450);
                popOut.ShowInTaskBar = true;
                popOut.ShowTitle = true;
                popOut.AlwaysOnTop = true;
                popOut.Mode = PopOutMode.Normal;
                popOut.InvokeReset();
            }
//#endif
        }
        #endregion

        #region Commander Methods
        private void OnCommandersUpdated(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                JournalCommanders.Clear();
                foreach (var journalCommander in _journalParserStore.JournalCommanders)
                {
                    JournalCommanders.Add(new JournalCommaderViewModel(journalCommander));
                };

                OnPropertyChanged(nameof(JournalCommanders));

                var commander = JournalCommanders.FirstOrDefault(x => x.Id == _settings.SelectedCommanderID);
                SelectedCommander = commander;
            });
        }
        #endregion

        #region Current System Methods
        private void CurrentSystemUpdated()
        {
            OnCurrentSystemUpdatedEvent?.Invoke(this, CurrentSystem);
            OnPropertyChanged(nameof(CurrentSystemName));
            OnPropertyChanged(nameof(CurrentSystemRegion));
        }

        private void OnCurrentSystemUpdated(object? sender, StarSystem? e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                InHyperSpace = false;
                currentSystem = null;
                OrganicSignals.ClearCollection();
                OnCurrentSystemUpdatedEvent?.Invoke(this, CurrentSystem);
                if (e is null)
                {
                    OnPropertyChanged(nameof(CurrentSystem));
                    OnPropertyChanged(nameof(OrganicSignals));
                    return;
                }

                try
                {
                    var system = new StarSystemViewModel(e, _settings, notificationStore);

                    if (e.SystemBodies != null && e.SystemBodies.Count > 0)
                    {
                        foreach (var body in e.SystemBodies)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var bodyVm = new SystemBodyViewModel(body, _settings);
                                if (bodyVm.AddOrganicItems())
                                {
                                    OrganicSignals.AddToCollection(bodyVm);
                                }

                                system.Bodies.AddToCollection(bodyVm);
                            });
                        }
                    }

                    currentSystem = system;
                    OnPropertyChanged(nameof(CurrentSystem));
                    OnPropertyChanged(nameof(OrganicSignals));
                }
                catch (Exception ex)
                {
                    App.Logger.Error(ex, "Current System Updated");
                }
                CurrentSystemUpdated();
            });
        }

        private void ExplorationData_OnAllBodiesDiscovered(object? sender, StarSystem? e)
        {
            if (e is not null && e.Address == CurrentSystem?.Address)
            {
                CurrentSystem.OnAllBodiesFound();
            }
            CurrentSystemUpdated();
        }

        private void ExplorationData_OnRouteUpdated(object? sender, List<StarSystem> e)
        {
            Route.ClearCollection();

            if (e is null || e.Count == 0)
            {
                OnPropertyChanged(nameof(Route));
                return;
            }

            var currentPos = CurrentSystem?.Position ?? new(0, 0, 0);
            var totalDistance = 0d;

            foreach (var system in e)
            {
                var distance = currentPos.DistanceFrom(system.Position);
                currentPos = system.Position;
                totalDistance += distance;
                var newSys = new StarSystemViewModel(system, _settings, notificationStore)
                {
                    JumpDistanceToSystem = $"{distance:N0} ly",
                    JumpDistanceRemaining = $"{totalDistance:N2} ly"
                };

                if (system.SystemBodies.Count > 0)
                {
                    newSys.Bodies.AddRangeToCollection(system.SystemBodies.Select(x => new SystemBodyViewModel(x, _settings)));
                }

                Route.AddToCollection(newSys);
            }

            OnPropertyChanged(nameof(Route));
            OnRouteUpdated?.Invoke(this, Route);
        }

        private void ExplorationData_OnBodyUpdated(object? sender, SystemBody e)
        {
            var system = GetSystem(e.Owner.Address);

            if (system != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var body = system.UpdateBody(e);

                    if (system.Address == _explorationData.CurrentSystem?.Address && body.BiologicalSignals > 0)
                    {
                        if (OrganicSignals.Contains(body) == false)
                            OrganicSignals.AddToCollection(body);
                    }
                    SelectedBody = body;
                    OnBodyUpdated?.Invoke(this, body);
                });
            }
        }

        private void ExplorationData_OnBodyTargeted(object? sender, SystemBody e)
        {
            if (CurrentSystem is null)
                return;
            var known = CurrentSystem.Bodies.FirstOrDefault(x => x.BodyID == e.BodyID);

            if (known != null)
            {
                SelectedBody = known;
            }
        }

        private void ExplorationData_OnSystemUpdatedFromEDSM(object? sender, StarSystem e)
        {
            var system = GetSystem(e.Address);
            system?.OnSystemUpdatedFromEDSM();

            if (currentSystem?.Address == e.Address)
            {
                OnCurrentSystemUpdated(null, e);
            }
        }

        private StarSystemViewModel? GetSystem(long address)
        {
            if (currentSystem?.Address == address)
            {
                return currentSystem;
            }

            var system = Route.FirstOrDefault(x => x.Address == address);

            if (system != null)
            {
                return system;
            }

            return null;
        }

        private void ExplorationData_OnBioDataUpdated(object? sender, OrganicScanItem e)
        {
            var known = OrganicSignals.FirstOrDefault(x => x.BodyID == e.Body.BodyID);

            if (known == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    known = new SystemBodyViewModel(e.Body, _settings);
                    known.AddOrganicItems();

                    known.UpdateOrganicInfo();
                    OnBioUpdated?.Invoke(this, known);
                });
                return;
            }

            known.UpdateOrganicInfo();
            OnBioUpdated?.Invoke(this, known);
        }

        private void ExplorationData_OnBodyBiosUpdated(object? sender, SystemBody e)
        {
            var known = OrganicSignals.FirstOrDefault(x => x.BodyID == e.BodyID);

            if (known == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    known = new SystemBodyViewModel(e, _settings);

                    if (known.OrganicScanItems.Count == 0)
                        known.AddOrganicItems();

                    known.UpdateOrganicInfo();
                    OnBioUpdated?.Invoke(this, known);
                });

                return;
            }

            if (known.OrganicScanItems.Count == 0)
                known.AddOrganicItems();

            known.UpdateOrganicInfo();
            OnBioUpdated?.Invoke(this, known);
        }

        private void ExplorationData_OnFSDJump(object? sender, string e)
        {
            InHyperSpace = true;
            HyperSpaceText = $"JUMPING TO {e.ToUpperInvariant()}";
            currentSystem = null;
            OrganicSignals.ClearCollection();
            CurrentSystemUpdated();
        }

        internal void OnExoMinValueChanged()
        {
            if (currentSystem == null)
                return;

            foreach(var body in currentSystem.Bodies)
            {
                body.UpdateOrganicHiddenStates();
            }
        }
        #endregion

        #region PopOut Window Methods
        private void OpenActivePopOuts()
        {
            var active = SettingsStore.GetCommanderPopOutParams(currentCommanderId).Where(x => x.Active).ToList();

            if (active.Count == 0)
                return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var allPopOuts = AppDomain
                              .CurrentDomain
                              .GetAssemblies()
                              .SelectMany(assembly => assembly.GetTypes())
                              .Where(type => typeof(PopOutBase)
                              .IsAssignableFrom(type))
                              .Where(type => !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface))
                              .Select(x => Activator.CreateInstance(x) as PopOutBase)
                              .Where(x => x != null && x.Title != string.Empty);

                foreach (var activePopout in active)
                {
                    var popOut = allPopOuts.Where(x => x is not null && x.Title == activePopout.Title).FirstOrDefault();

                    if (popOut is null)
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(popOut.GetType()) is not PopOutBase newPopOut)
                        continue;

                    OpenPopout(newPopOut, activePopout.Count);
                }
            });
        }
        public void OpenPopout(PopOutBase popOut, int count = 0)
        {
            var popOutParams = SettingsStore.GetParams(popOut, count, currentCommanderId);
            popOut.ApplyParams(popOutParams);
            popOut.DataContext = this;

            var popOutWindow = new PopOutWindow(popOut)
            {
                Owner = Application.Current.MainWindow,
            };

            popOutWindow.Show();
            ActivePopOuts.Add(popOut);
        }
        internal void OnPopOutClose(PopOutBase popOutBase)
        {
            SettingsStore.SaveParams(popOutBase, false, currentCommanderId);
            ActivePopOuts.Remove(popOutBase);
        }
        #endregion

        public void OnClose()
        {
            if (ActivePopOuts.Count > 0)
            {
                foreach (var popout in ActivePopOuts)
                {
                    SettingsStore.SaveParams(popout, true, currentCommanderId);
                }
            }

            notificationStore.Dispose();
            SettingsStore.SaveSettings();
            spanshCsvStore.SaveCSVs();
        }
    }
}
