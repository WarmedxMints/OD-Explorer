using ODExplorer.Models;
using ODUtils.Dialogs.ViewModels;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class IgnoredSystemsViewModel(IgnoredSystem ignoredSystem) : OdViewModelBase
    {
        private bool restore;

        public long Address => ignoredSystem.Address;
        public string Name => ignoredSystem.Name;
        public int CmdrId => ignoredSystem.CmdrId;
        public bool Restore
        {
            get => restore;
            set
            {
                restore = value;
                OnPropertyChanged(nameof(Restore));
            }
        }
    }
}
