using ODExplorer.Models;
using ODExplorer.ViewModels.ModelVMs;
using ODExplorer.ViewModels.ViewVMs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ODExplorer.Controls
{
    [Flags]
    public enum GridFiltering
    {
        Edsm = 1 << 0,
        WorthMapping = 1 << 1,
        GeoSignals = 1 << 2,
        BioSignals = 1 << 3,
        All = ~(-1 << 4)
    }
    /// <summary>
    /// Interaction logic for SystemBodiesOverlay.xaml
    /// </summary>
    public partial class SystemBodiesOverlay : PopOutBase
    {
        public SystemBodiesOverlay()
        {
            Title = "Bodies Overlay";
            InitializeComponent();
            Loaded += SystemBodiesOverlay_Loaded;
            Unloaded += SystemBodiesOverlay_Unloaded;
        }

        public override string Title { get; protected set; }

        private GridFiltering filtering = GridFiltering.All;
        private ListCollectionView? bodiesView;

        public GridFiltering Filtering
        {
            get => filtering;
            set
            {
                filtering = value;
                bodiesView?.Refresh();
                OnPropertyChanged(nameof(Filtering));
            }
        }

        public override object? AdditionalSettings
        {
            get => Filtering;
            protected set
            {
                if (value == null)
                {
                    Filtering = GridFiltering.All;
                    return;
                }
                if (value is GridFiltering filter)
                {
                    Filtering = filter;
                    return;
                }
                if (value is long int64)
                {
                    Filtering = (GridFiltering)int64;
                    return;
                }
                Filtering = GridFiltering.All;
            }
        }

        private void SystemBodiesOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel model)
            {
                model.OnCurrentSystemUpdatedEvent += Model_OnCurrentSystemUpdatedEvent;

                if (model.CurrentSystem is null)
                {
                    DataGrid.ItemsSource = null;
                    bodiesView = null;
                    return;
                }

                CreateCVS(model.CurrentSystem);
            }
        }

        private void SystemBodiesOverlay_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel model)
            {
                model.OnCurrentSystemUpdatedEvent -= Model_OnCurrentSystemUpdatedEvent;
            }
        }

        private void Model_OnCurrentSystemUpdatedEvent(object? sender, StarSystemViewModel? e)
        {
            if (e is null || e.Bodies is null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataGrid.ItemsSource = null;
                    DataGrid.Items.Refresh();
                    bodiesView = null;
                });
                return;
            }
            CreateCVS(e);
        }

        private void CreateCVS(StarSystemViewModel e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DataGrid.ItemsSource = null;
                bodiesView = new ListCollectionView(e.Bodies)
                {
                    CustomSort = new SystemBodyViewModelDistanceFromArrivalComparer(true)
                };
                bodiesView.LiveFilteringProperties.Add("WorthMapping");
                bodiesView.LiveFilteringProperties.Add("IsEdsmVb");
                bodiesView.LiveFilteringProperties.Add("GeologicalSignals");
                bodiesView.LiveFilteringProperties.Add("BiologicalSignals");
                bodiesView.IsLiveFiltering = true;
                bodiesView.Filter += BodiesFilter;
                bodiesView.CurrentChanged += BodiesView_CurrentChanged;
                bodiesView.Refresh();
                DataGrid.ItemsSource = bodiesView;
            });
        }

        private void BodiesView_CurrentChanged(object? sender, EventArgs e)
        {
        }

        private bool BodiesFilter(object obj)
        {
            if (obj is SystemBodyViewModel body && body is not null)
            {
                return
                    Filtering.HasFlag(GridFiltering.WorthMapping) && body.WorthMapping
                    || Filtering.HasFlag(GridFiltering.Edsm) && body.IsEdsmVb
                    || Filtering.HasFlag(GridFiltering.BioSignals) && body.BiologicalSignals > 0 && body.Status != ODUtils.Models.DiscoveryStatus.MappedByUser
                    || Filtering.HasFlag(GridFiltering.GeoSignals) && body.GeologicalSignals > 0 && body.Status != ODUtils.Models.DiscoveryStatus.MappedByUser;
                ;
            }
            return false;
        }

        public override void ApplyStyles(PopOutMode mode, bool mouseEnter)
        {
            var settingsVis = mouseEnter ? Visibility.Visible : Visibility.Collapsed;

            SettingsGrid.Visibility = settingsVis;

            switch (mode)
            {
                case PopOutMode.Normal:
                    SystemBodyGrid.Margin = new(0, 10, 0, 0);
                    break;
                case PopOutMode.Opaque:
                case PopOutMode.Transparent:
                    SystemBodyGrid.Margin = new(0, 0, 0, 0);
                    break;
            }
        }
    }
}
