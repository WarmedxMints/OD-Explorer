using ODExplorer.ViewModels.ViewVMs;
using ODUtils.Dialogs;
using ODUtils.Windows;
using System;
using System.Windows;

namespace ODExplorer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        public MainWindow(MainViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel model)
            {
                model.OnClose();
            }
        }

        protected override void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                WindowState = vm.SettingsStore.WindowPosition.State;
                vm.OnMessageBoxRequested += Vm_OnMessageBoxRequested;
                vm.AdjustUiScaleEvent += Vm_OnAdjustUiScale;
                vm.WindowReset += Vm_WindowReset;
            }
            base.WindowBase_Loaded(sender, e);
        }

        private void Vm_WindowReset(object? sender, EventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void Vm_OnAdjustUiScale(object? sender, EventArgs e)
        {
            var adjuster = new UiScaleAdjustment()
            {
                DataContext = this.DataContext,
                Owner = this
            };
            adjuster.ShowDialog();
        }

        private async void Vm_OnMessageBoxRequested(object? sender, Models.MessageBoxEventArgsAsync e)
        {
            var msgBox = ODMessageBox.Show(this,
                                         e.Title,
                                         e.Message,
                                         e.Buttons);

            if (msgBox != MessageBoxResult.Yes)
            {
                if (e.CallbackNo != null)
                    await e.CallbackNo.Invoke();
                return;
            }

            if (e.CallbackYes != null)
                await e.CallbackYes.Invoke();
        }

        protected override void StateChangeRaised(object? sender, EventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.SettingsStore.WindowPosition.State = WindowState;
            }
            base.StateChangeRaised(sender, e);
        }
    }
}
