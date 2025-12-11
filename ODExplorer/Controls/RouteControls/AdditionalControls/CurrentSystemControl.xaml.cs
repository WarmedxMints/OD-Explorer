using ODExplorer.Models;
using ODExplorer.ViewModels.ModelVMs;
using ODExplorer.ViewModels.ViewVMs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for CurrentSystemControl.xaml
    /// </summary>
    public partial class CurrentSystemControl : UserControl
    {
        public SystemBodyViewModel SelectedBody
        {
            get { return (SystemBodyViewModel)GetValue(SelectedBodyProperty); }
            set { SetValue(SelectedBodyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedBodyProperty =
            DependencyProperty.Register("SelectedBody", typeof(SystemBodyViewModel), typeof(CurrentSystemControl), new PropertyMetadata());


        public CurrentSystemControl()
        {
            InitializeComponent();
            Loaded += CurrentSystemControl_Loaded;
        }

        private void CurrentSystemControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not CartographicViewModel viewModel)
            {
                return;
            }

            var GridSettings = viewModel.CurrentSystemGridSettings;

            if (GridSettings != null)
            {
                DataGrid.Columns[0].Visibility = BoolToVis(GridSettings.ShowBodyIcon);
                DataGrid.Columns[1].Visibility = BoolToVis(GridSettings.ShowBodyId);
                BodyHeaderGrid.ColumnDefinitions[0].Width = GridSettings.ShowBodyIcon ? new GridLength(DataGrid.Columns[0].ActualWidth) : new GridLength(0);
                BodyHeaderGrid.ColumnDefinitions[1].Width = GridSettings.ShowBodyId ? new GridLength(DataGrid.Columns[1].ActualWidth) : new GridLength(0);

                DataGrid.Columns[4].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.AtmosphereType));
                DataGrid.Columns[5].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.SurfaceTemp));
                DataGrid.Columns[6].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.SurfacePressure));
                DataGrid.Columns[7].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.WasDiscovered));
                DataGrid.Columns[8].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.Foorfall));
                DataGrid.Columns[9].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.Unmapped));
                DataGrid.Columns[10].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.Terraformable));
                DataGrid.Columns[11].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.HasRings));
                DataGrid.Columns[12].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.GeoSignals));
                DataGrid.Columns[13].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.BioSignals));
                DataGrid.Columns[14].Visibility = BoolToVis(GridSettings.InfoDisplayOptions.HasFlag(BodyInfoIconDisplay.SurfaceGravity));                
            }
        }

        private static Visibility BoolToVis(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (sender is not Control control)
            {
                return;
            }
            e.Handled = true;
            var wheelArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = control
            };
            var parent = control.Parent as UIElement;
            parent?.RaiseEvent(wheelArgs);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedBody is not null)
                DataGrid.ScrollIntoView(SelectedBody);
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

                if (SelectedBody is not null && SelectedBody.IsNonBody == false)
                    dataGrid.ScrollIntoView(SelectedBody);
            }
        }
    }
}
