using ODExplorer.Models;
using ODUtils.ViewModelNavigation;
using System;
using System.Windows.Input;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class NavigationViewModel(OdNavigationService<LoadingViewModel> loadingCommand,
                                            OdNavigationService<CartographicViewModel> cartoView,
                                            OdNavigationService<OrganicViewModel> organicView,
                                            OdNavigationService<SettingsViewModel> settingView,
                                            OdNavigationService<DisplaySettingsViewModel> displaySettingsView,
                                            OdNavigationService<CartoDetailsViewModel> cartoDetailsView,
                                            OdNavigationService<SpanshViewModel> spanshView,
                                            OdNavigationService<EdAstroViewModel> edAstroView)
    {
        public ICommand LoadingViewCommand { get; } = new OdNavigateCommand<LoadingViewModel>(loadingCommand);
        public ICommand CartographicViewCommand { get; } = new OdNavigateCommand<CartographicViewModel>(cartoView);
        public ICommand OrganicViewCommand { get; } = new OdNavigateCommand<OrganicViewModel>(organicView);
        public ICommand SettingsViewCommand { get; } = new OdNavigateCommand<SettingsViewModel>(settingView);
        public ICommand DisplaySettingsViewCommand { get; } = new OdNavigateCommand<DisplaySettingsViewModel>(displaySettingsView);
        public ICommand CartoDetailsViewCommand { get; } = new OdNavigateCommand<CartoDetailsViewModel>(cartoDetailsView);
        public ICommand SpanshViewCommand { get; } = new OdNavigateCommand<SpanshViewModel>(spanshView);
        public ICommand EdAstroViewCommand { get; } = new OdNavigateCommand<EdAstroViewModel>(edAstroView);

        public event EventHandler<MessageBoxEventArgsAsync>? MessagoBoxRequested;
        internal void InvokeMessageBox(MessageBoxEventArgsAsync args)
        {
            MessagoBoxRequested?.Invoke(this, args);
        }
    }
}
