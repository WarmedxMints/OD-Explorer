using ODExplorer.ViewModels.ModelVMs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for OrganicScanItemControl.xaml
    /// </summary>
    public partial class OrganicScanItemControl : UserControl, INotifyPropertyChanged
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
        public ObservableCollection<OrganicScanItemViewModel> OrganicDetails
        {
            get { return (ObservableCollection<OrganicScanItemViewModel>)GetValue(OrganicDetailsProperty); }
            set { SetValue(OrganicDetailsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrganicDetailsProperty =
            DependencyProperty.Register("OrganicDetails", typeof(ObservableCollection<OrganicScanItemViewModel>), typeof(OrganicScanItemControl), new PropertyMetadata());

        private ObservableCollection<OrganicScanItemViewModel>? _OrganicDetails;

        private ObservableCollection<OrganicTotalsViewModel> totals = [];
        public ObservableCollection<OrganicTotalsViewModel> Totals { get => totals; set { totals = value; OnPropertyChanged(); } }

        private string? totalValue;
        public string? TotalValue { get => totalValue; set { totalValue = value; OnPropertyChanged(nameof(TotalValue)); } }

        private string? totalCount;
        public string? TotalCount { get => totalCount; set { totalCount = value; OnPropertyChanged(nameof(TotalCount)); } }

        private string? totalBonus;
        public string? TotalBonus { get => totalBonus; set { totalBonus = value; OnPropertyChanged(nameof(TotalBonus)); } }

        public OrganicScanItemControl()
        {
            InitializeComponent();
            Loaded += OrganicScanItemControl_Loaded;
            Unloaded += OrganicScanItemControl_Unloaded;
        }

        private void OrganicScanItemControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (OrganicDetails != null)
            {
                _OrganicDetails = OrganicDetails;
                _OrganicDetails.CollectionChanged += _OrganicDetails_CollectionChanged;
                BuildTotals();
            }
        }

        private void OrganicScanItemControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_OrganicDetails != null)
            {
                _OrganicDetails.CollectionChanged -= _OrganicDetails_CollectionChanged;
            }
        }

        private void _OrganicDetails_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BuildTotals();
        }

        private void BuildTotals()
        {
            Totals.Clear();

            var totals = OrganicDetails.OrderBy(x => x.EnglishName).GroupBy(x => x.EnglishName);

            foreach (var item in totals)
            {
                var total = new OrganicTotalsViewModel()
                {
                    EnglishName = item.Key,
                    Count = item.Count(),
                    Value = item.Sum(x => x.Value),
                    Bonus = item.Sum(x => x.Bonus)
                };

                Totals.Add(total);
            }

            TotalValue = Totals.Sum(x => x.Value).ToString("N0");
            TotalCount = Totals.Sum(x => x.Count).ToString("N0");
            TotalBonus = Totals.Sum(x => x.Bonus).ToString("N0");
        }
    }
}
