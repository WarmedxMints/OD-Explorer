using ODExplorer.Models;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Extensions;
using ODUtils.Models;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class OrganicCheckListVariantViewModel(OrganicChechListItemVariant? variant) : OdViewModelBase
    {
        public string CodexValue => variant?.VaritantCodex ?? string.Empty;
        public string LocalName => variant?.LocalName ?? string.Empty;
        public string Region => variant?.Region.GetEnumDescription() ?? string.Empty;
        public string ScanStage => variant?.State.GetEnumDescription() ?? string.Empty;
        public OrganicScanState StageValue => variant?.State ?? OrganicScanState.Unavailable;
    }
}
