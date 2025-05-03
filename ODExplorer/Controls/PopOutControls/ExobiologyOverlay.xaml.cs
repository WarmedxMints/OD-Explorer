using ODExplorer.Models;
using ODExplorer.ViewModels.ModelVMs;
using ODExplorer.ViewModels.ViewVMs;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {

            if (sender is DataGrid dataGrid)
                dataGrid.ItemContainerGenerator.StatusChanged += (container, e) => ItemContainerGenerator_StatusChanged(container, dataGrid);
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
                dataGrid.ItemContainerGenerator.StatusChanged -= (container, e) => ItemContainerGenerator_StatusChanged(container, dataGrid);
        }

        private void ItemContainerGenerator_StatusChanged(object? sender, DataGrid dataGrid)
        {
            if (sender is ItemContainerGenerator icg && icg.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (DataGridColumn col in dataGrid.Columns)
                {
                    DataGridLength width = col.Width;
                    col.Width = 0;
                    col.Width = width;
                }
            }
        }

        private void ToggleHiddenBtnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SystemBodyViewModel body)
            {
                body.ToggleHiddenBios();
            }
        }
    }
}
