using ODExplorer.Stores;
using ODUtils.Commands;
using ODUtils.Dialogs.ViewModels;
using System.Windows.Input;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class LoadingViewModel : OdViewModelBase
    {
        public LoadingViewModel(JournalParserStore journalParseStore, SettingsStore settingsStore)
        {
            journalStore = journalParseStore;
            journalStore.OnJournalStoreStatusChange += JournalStore_OnStatusChange;

            OpenPayPal = new RelayCommand(OnOpenPayPal);
            OpenGitHub = new RelayCommand(OnOpenGitHub);
            if (settingsStore.SelectedCommanderID >= 0)
                journalStore.ReadNewCommander(settingsStore.SelectedCommanderID);
        }

        private readonly JournalParserStore journalStore;

        public ICommand OpenPayPal { get; }
        public ICommand OpenGitHub { get; }

        private string statusText = string.Empty;
        public string StatusText
        {
            get => statusText;
            set
            {
                statusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private void OnOpenPayPal(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&no_recurring=0&item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&currency_code=GBP");
        }

        private void OnOpenGitHub(object? obj)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl("https://github.com/WarmedxMints/OD-Explorer");
        }

        public override void Dispose()
        {
            journalStore.OnJournalStoreStatusChange -= JournalStore_OnStatusChange;
        }

        private void JournalStore_OnStatusChange(object? sender, string? e)
        {
            StatusText = e ?? string.Empty;
        }
    }
}
