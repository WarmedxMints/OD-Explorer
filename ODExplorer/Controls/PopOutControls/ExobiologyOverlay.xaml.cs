using ODExplorer.Models;
using ODExplorer.ViewModels.ModelVMs;
using ODExplorer.ViewModels.ViewVMs;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for ExobiologyOverlay.xaml
    /// </summary>
    public partial class ExobiologyOverlay : PopOutBase
    {
        public ExobiologyOverlay()
        {
            Title = "Exobiology Overlay";
            InitializeComponent();
            Loaded += ExobiologyOverlay_Loaded;
            Unloaded += ExobiologyOverlay_Unloaded;
        }

        private ListCollectionView? bodiesView;

        private void ExobiologyOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel model)
            {
                model.OnCurrentSystemUpdatedEvent += Model_OnCurrentSystemUpdatedEvent;

                if (model.CurrentSystem is null)
                {
                    BioBodiesGrid.ItemsSource = null;
                    bodiesView = null;
                    return;
                }

                CreateCVS(model.OrganicSignals);
            }
        }

        private void ExobiologyOverlay_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel model)
            {
                model.OnCurrentSystemUpdatedEvent -= Model_OnCurrentSystemUpdatedEvent;
            }
        }

        private void Model_OnCurrentSystemUpdatedEvent(object? sender, StarSystemViewModel? e)
        {
            if (e is null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BioBodiesGrid.ItemsSource = null;
                    bodiesView = null;
                    BioBodiesGrid.SelectedItem = null;
                    BioBodiesGrid.Items.Refresh();
                });
                return;
            }
            if (sender is MainViewModel model)
            {
                CreateCVS(model.OrganicSignals);
            }
        }

        private void CreateCVS(ObservableCollection<SystemBodyViewModel> organicSignals)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BioBodiesGrid.ItemsSource = null;
                bodiesView = new ListCollectionView(organicSignals)
                {
                    CustomSort = new SystemBodyViewModelDistanceFromArrivalComparer(true)
                };
                bodiesView.CurrentChanged += BodiesView_CurrentChanged;
                bodiesView.Refresh();

                BioBodiesGrid.ItemsSource = bodiesView;
            });
        }

        private void BodiesView_CurrentChanged(object? sender, EventArgs e)
        {
            if (sender is ListCollectionView view && view.CurrentItem is SystemBodyViewModel body)
            {
                OrganicGrid.ItemsSource = body.OrganicScanItems;
            }
        }
    }
}
