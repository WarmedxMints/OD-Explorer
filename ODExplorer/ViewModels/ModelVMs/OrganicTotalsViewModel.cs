using ODUtils.Dialogs.ViewModels;
using ODUtils.Extensions;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class OrganicTotalsViewModel : OdViewModelBase
    {
        public required string EnglishName { get; set; }

        public int Count { get; set; }
        public string CountString => Count.ToString("N0");

        public long Value { get; set; }
        public string ValueString => Value.FormatNumber();

        public long Bonus { get; set; }
        public string BonusString => Bonus.FormatNumber();
    }
}
