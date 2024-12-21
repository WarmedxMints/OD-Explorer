using ODUtils.Dialogs.ViewModels;
using ODUtils.Spansh;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class BodiesInfoViewModel(BodiesInfo info) : OdViewModelBase
    {
        public BodiesInfo Info { get; } = info;
        public string? Body => Info.Body;
        public string? Distance => Info.Distance;
        public string? Property1 => Info.Property1;
    }
}
