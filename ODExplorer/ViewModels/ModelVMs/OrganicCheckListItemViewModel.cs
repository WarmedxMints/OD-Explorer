using ODUtils.Dialogs.ViewModels;
using ODUtils.Extensions;
using ODUtils.Models;
using System.Collections.Generic;
using System.Linq;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class OrganicCheckListItemViewModel : OdViewModelBase
    {
        public string CodexValue { get; set; } = string.Empty;

        private string name = string.Empty;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private OrganicScanState state;
        public OrganicScanState State
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public string StateString
        {
            get
            {
                if (state == OrganicScanState.None)
                    return string.Empty;
                return state.GetEnumDescription();
            }
        }

        private List<OrganicCheckListVariantViewModel> variants = [];
        public List<OrganicCheckListVariantViewModel> Variants
        {
            get => variants;
            set
            {
                variants = value;
                OnPropertyChanged(nameof(Variants));
            }
        }

        public string VariantCount
        {
            get
            {
                var count = Variants.Where(x => x.StageValue >= OrganicScanState.Discovered).DistinctBy(x => x.CodexValue).Count();
                return $"{count}";
            }
        }

        public string TotalVariants
        {
            get
            {
                var variantCount = Variants.Count;
                return $"{variantCount}";
            }
        }

        public void UpdateCounts()
        {
            OnPropertyChanged(nameof(VariantCount));
            OnPropertyChanged(nameof(TotalVariants));
        }
    }
}
