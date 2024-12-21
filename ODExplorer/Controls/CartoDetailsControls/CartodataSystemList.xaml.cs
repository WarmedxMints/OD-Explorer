using ODExplorer.ViewModels.ModelVMs;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for CardataSystemList.xaml
    /// </summary>
    public partial class CartodataSystemList : UserControl, INotifyPropertyChanged
    {
        #region Property Changed
        // Declare the event
        public event PropertyChangedEventHandler? PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            if (Application.Current is null)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
                catch (Exception e) { Console.WriteLine(e.Message); }
            });
        }
        #endregion
        public ObservableCollection<StarSystemViewModel> Systems
        {
            get { return (ObservableCollection<StarSystemViewModel>)GetValue(SystemsProperty); }
            set { SetValue(SystemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SystemsProperty =
            DependencyProperty.Register("Systems", typeof(ObservableCollection<StarSystemViewModel>), typeof(CartodataSystemList), new PropertyMetadata(null));

        public bool ShowIgnoreButton
        {
            get { return (bool)GetValue(ShowIgnoreButtonProperty); }
            set { SetValue(ShowIgnoreButtonProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowIgnoreButton.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowIgnoreButtonProperty =
            DependencyProperty.Register("ShowIgnoreButton", typeof(bool), typeof(CartodataSystemList), new PropertyMetadata(false));

        public StarSystemViewModel? SelectedSystem
        {
            get { return (StarSystemViewModel?)GetValue(SelectedSystemProperty); }
            set { SetValue(SelectedSystemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedSystem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedSystemProperty =
            DependencyProperty.Register("SelectedSystem", typeof(StarSystemViewModel), typeof(CartodataSystemList), new PropertyMetadata());

        private ICollectionView? selectedSystemBodies;
        public ICollectionView? SelectedSystemBodies
        {
            get => selectedSystemBodies;
            set
            {
                selectedSystemBodies = value;
                OnPropertyChanged(nameof(SelectedSystemBodies));
            }
        }
        public string TotalValue { get; set; } = "0";

        private SystemBodyViewModel? selectedBody;
        public SystemBodyViewModel? SelectedBody
        {
            get => selectedBody;
            set
            {
                selectedBody = value;
                OnPropertyChanged(nameof(SelectedBody));
            }
        }

        public string IgnoreSystemText
        {
            get
            {
                if (SelectedSystem is null)
                    return "No System Selected";

                return $"Add {SelectedSystem.Name} To Ignore List";
            }
        }

        private ObservableCollection<StarSystemViewModel>? systemsReference;

        public CartodataSystemList()
        {
            InitializeComponent();
            Loaded += CartoDataSystemList_Loaded;
            Unloaded += CartoDataSystemList_Unloaded;
        }

        private void CartoDataSystemList_Unloaded(object sender, RoutedEventArgs e)
        {
            if (systemsReference is null)
                return;

            systemsReference.CollectionChanged -= OnSystemsChanged;
        }

        private void CartoDataSystemList_Loaded(object sender, RoutedEventArgs e)
        {
            if (Systems is null)
                return;

            systemsReference = Systems;

            systemsReference.CollectionChanged += OnSystemsChanged;
            TotalValue = systemsReference.Sum(x => x.DataValue).ToString("N0");
            OnPropertyChanged(nameof(TotalValue));
        }

        private void OnSystemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            TotalValue = Systems.Sum(x => x.DataValue).ToString("N0");
            OnPropertyChanged(nameof(TotalValue));
        }

        private void SystemGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedSystem != null)
            {
                SelectedBody = SelectedSystem?.Bodies?.FirstOrDefault();
                OnPropertyChanged(nameof(IgnoreSystemText));
                if (SelectedSystem?.Bodies != null)
                {
                    SelectedSystemBodies = CollectionViewSource.GetDefaultView(SelectedSystem.Bodies);
                    BodiesGrid.ItemsSource = SelectedSystemBodies;
                    SelectedSystemBodies.Filter = CollectionViewSource_Filter;
                    return;
                }

                BodiesGrid.ItemsSource = null;
            }
        }

        private void SystemGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedSystem is not null)
            {
                var sys = Systems.FirstOrDefault(x => x.Address == SelectedSystem.Address);
                SystemGrid.ScrollIntoView(sys);
                SelectedSystem = sys;
                if (sys != null && sys.Bodies != null)
                {
                    SelectedSystemBodies = CollectionViewSource.GetDefaultView(sys.Bodies);
                    BodiesGrid.ItemsSource = SelectedSystemBodies;
                    SelectedSystemBodies.Filter = CollectionViewSource_Filter;
                    return;
                }

                BodiesGrid.ItemsSource = null;
            }
        }

        private bool CollectionViewSource_Filter(object obj)
        {
            if (obj is SystemBodyViewModel body)
            {
                return body.Name.Equals("BaryCentre", StringComparison.OrdinalIgnoreCase) == false;
            }
            return false;
        }
    }
}
