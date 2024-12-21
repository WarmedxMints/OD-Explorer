using ODUtils.Dialogs.ViewModels;
using ODUtils.Exobiology;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class OrganicInfoViewModel(OrganicInfo organicInfo) : OdViewModelBase
    {
        public string EnglishName => organicInfo.EnglishName;
        public long Value { get; } = organicInfo.Value;
        public int ColonyRange { get; } = organicInfo.ColonyRange;
    }
}
