using ODExplorer.Controls;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODExplorer.ViewModels.ModelVMs;
using ODUtils.Commands;
using ODUtils.Dialogs.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class CartographicViewModel : OdViewModelBase
    {
        public CartographicViewModel(ExplorationDataStore explorationData,
                             JournalParserStore parserStore,
                             SettingsStore settingsStore,
                             MainViewModel mainView)
        {
            this.explorationData = explorationData;
            this.parserStore = parserStore;
            this.settingsStore = settingsStore;
            this.mainView = mainView;

            this.mainView.OnCurrentSystemUpdatedEvent += MainView_OnCurrentSystemUpdatedEvent;
            this.mainView.OnRouteUpdated += MainView_OnRouteUpdated;
            this.mainView.OnBodyUpdated += MainView_OnBodyUpdated;
            this.mainView.OnBioUpdated += MainView_OnBioUpdated;
            this.mainView.OnSelectedBodyUpdated += MainView_OnSelectedBodyUpdated;
            this.explorationData.OnFSDJump += ExplorationData_OnFSDJump;

            SwitchView = new RelayCommand<CartoViewState>(OnSwitchView, (viewState) => CurrentState != viewState);
            OpenValuableBodiesPopOut = new RelayCommand(OnOpenValuableBodiesPopOut);
            OpenExobiologyPopOut = new RelayCommand(OnOpenExobiologyPopOut);

            Application.Current.Dispatcher.Invoke(() =>
            {
                if(CurrentSystem != null) 
                    currentSystemBodies = new(CurrentSystem?.Bodies);

                _ = Task.Factory.StartNew(() =>
                {
                    MainView_OnCurrentSystemUpdatedEvent(null, this.mainView.CurrentSystem);
                    MainView_OnRouteUpdated("Start up", this.mainView.Route);
                }).ConfigureAwait(true);
            });
        }

        private readonly ExplorationDataStore explorationData;
        private readonly JournalParserStore parserStore;
        private readonly SettingsStore settingsStore;
        private readonly MainViewModel mainView;

        public StarSystemViewModel? CurrentSystem => mainView.CurrentSystem;
        public ObservableCollection<StarSystemViewModel> Route => mainView.Route;
        public ObservableCollection<SystemBodyViewModel> OrganicSignals => mainView.OrganicSignals;

        public SystemBodyViewModel? SelectedBody
        {
            get
            {
                return mainView.SelectedBody;
            }
            set
            {
                mainView.SelectedBody = value;
                OnPropertyChanged(nameof(SelectedBody));
            }
        }

        private ListCollectionView? currentSystemBodies;
        public ListCollectionView? CurrentSystemBodies
        {
            get
            {
                return currentSystemBodies;
            }
            set
            {
                currentSystemBodies = value;
                OnPropertyChanged(nameof(CurrentSystemBodies));
            }
        }

        public CartoViewState CurrentState
        {
            get
            {
                if (parserStore.IsLive == false)
                {
                    return CartoViewState.None;
                }
                return settingsStore.CartoViewState;
            }
            set
            {
                settingsStore.CartoViewState = value;
                _ = Task.Factory.StartNew(() =>
                {
                    OnPropertyChanged(nameof(CurrentState));
                });
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

        public string CartoValue => explorationData.GetUnsoldCartoSystems().Sum(x => x.CommanderValue).ToString("N0");
        public string ExoValue => explorationData.GetUnsoldExoValueString();
        public bool FilterUnconfirmedBios => settingsStore.SystemGridSetting.FilterUnconfirmedBios;
        public SystemGridSettings CurrentSystemGridSettings => settingsStore.SystemGridSetting;
        public GridSize HorizontalViewGridSize => settingsStore.CartoHorizontalGridSize;
        public GridSize DetailedViewGridSize => settingsStore.CartoDetailedGridSize;
        public GridSize ExtendedBodyInfoGridSize => settingsStore.ExtendedBodyInfoGridSize;
        public GridSize CurrentExoGridSize => settingsStore.CurrentExoGridSize;

        private void MainView_OnSelectedBodyUpdated(object? sender, SystemBodyViewModel? e)
        {
            OnPropertyChanged(nameof(SelectedBody));
        }

        private void MainView_OnBioUpdated(object? sender, SystemBodyViewModel e)
        {
            OnPropertyChanged(nameof(ExoValue));
        }

        private void MainView_OnBodyUpdated(object? sender, SystemBodyViewModel e)
        {
            if (settingsStore.SystemGridSetting.IgnoreNonBodies && e.IsNonBody)
            {
                OnPropertyChanged(nameof(CartoValue));
                RefreshBodiesView();
                return;
            }
            OnPropertyChanged(nameof(SelectedBody));
            RefreshBodiesView();
            OnPropertyChanged(nameof(CartoValue));
            if (e.BiologicalSignals > 0)
                OnPropertyChanged(nameof(ExoValue));
        }

        private void MainView_OnRouteUpdated(object? sender, ObservableCollection<StarSystemViewModel> e)
        {
            OnPropertyChanged(nameof(Route));
        }

        private void MainView_OnCurrentSystemUpdatedEvent(object? sender, StarSystemViewModel? e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                InHyperSpace = false;
                HyperSpaceText = string.Empty;

                OnPropertyChanged(nameof(CurrentSystem));
                OnPropertyChanged(nameof(OrganicSignals));
                ApplyBodyCollectionViewSourceSorting();
            });
        }

        private void ApplyBodyCollectionViewSourceSorting()
        {
            if (CurrentSystem is null)
            {
                CurrentSystemBodies = null;
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var gridSettings = settingsStore.SystemGridSetting;
                currentSystemBodies = new ListCollectionView(CurrentSystem?.Bodies)
                {
                    IsLiveSorting = true,
                    CustomSort = new SystemBodyViewModelMainComparer(gridSettings)
                };

                currentSystemBodies.LiveSortingProperties.Add("IsStar");
                currentSystemBodies.LiveSortingProperties.Add("IsEdsmVb");
                currentSystemBodies.LiveSortingProperties.Add("SurfaceGravity");
                currentSystemBodies.LiveSortingProperties.Add("DistanceFromArrival");
                currentSystemBodies.LiveSortingProperties.Add("PlanetClassDescription");
                currentSystemBodies.LiveSortingProperties.Add("Name");
                currentSystemBodies.LiveSortingProperties.Add("BodyID");
                currentSystemBodies.LiveSortingProperties.Add("BiologicalSignals");
                currentSystemBodies.LiveSortingProperties.Add("GeologicalSignals");
                currentSystemBodies.LiveSortingProperties.Add("WorthMapping");
                currentSystemBodies.LiveSortingProperties.Add("MappedValueActual");

                if (gridSettings.IgnoreNonBodies)
                {
                    currentSystemBodies.Filter += IgnoreSystemBodiesFilter;
                }

                currentSystemBodies?.Refresh();
                OnPropertyChanged(nameof(CurrentSystemBodies));
            });
        }

        private bool IgnoreSystemBodiesFilter(object obj)
        {
            if (obj is SystemBodyViewModel body)
            {
                return !body.IsNonBody;
               
            }
            return false;
        }

        private void IgnoreSystemBodiesFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is SystemBodyViewModel body)
            {
                e.Accepted = !body.IsNonBody;
                return;
            }
            e.Accepted = false;
        }

        public override void Dispose()
        {

            this.mainView.OnCurrentSystemUpdatedEvent -= MainView_OnCurrentSystemUpdatedEvent;
            this.mainView.OnRouteUpdated -= MainView_OnRouteUpdated;
            this.mainView.OnBodyUpdated -= MainView_OnBodyUpdated;
            this.mainView.OnBioUpdated -= MainView_OnBioUpdated;
            this.mainView.OnSelectedBodyUpdated -= MainView_OnSelectedBodyUpdated;

            this.explorationData.OnFSDJump -= ExplorationData_OnFSDJump;
        }

        #region Commands
        public ICommand SwitchView { get; }
        public ICommand OpenValuableBodiesPopOut { get; }
        public ICommand OpenExobiologyPopOut { get; }

        private void OnSwitchView(CartoViewState state)
        {
            CurrentState = state;
        }

        private void OnOpenValuableBodiesPopOut(object? obj)
        {
            var popOut = new SystemBodiesOverlay();
            mainView.OpenPopout(popOut);
        }

        private void OnOpenExobiologyPopOut(object? obj)
        {
            var popOut = new ExobiologyOverlay();
            mainView.OpenPopout(popOut);
        }
        #endregion       

        private void RefreshBodiesView()
        {
            Application.Current.Dispatcher.Invoke(() => currentSystemBodies?.Refresh());
        }

        private void ExplorationData_OnFSDJump(object? sender, string e)
        {
            HyperSpaceText = $"JUMPING TO {e.ToUpperInvariant()}";
            InHyperSpace = true;
            RefreshBodiesView();
        }
    }
}
