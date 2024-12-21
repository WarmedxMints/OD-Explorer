using EliteJournalReader;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Exobiology;
using ODUtils.Models;
using System.Linq;

namespace ODExplorer.ViewModels.ModelVMs
{
    public class OrganicVariantViewModel(OrganicVariant variant, OrganicScanItemViewModel vm) : OdViewModelBase
    {
        private readonly OrganicVariant Variant = variant;
        private readonly OrganicScanItemViewModel vm = vm;

        public string VariantCodex => Variant.VariantCodex;
        public string EnglishName => Variant.EnglishName;
        public string LocalName => Variant.LocalName;
        public VariantColours Colour => Variant.Colour;
        public PlanetMaterial Material => Variant.Material;
        public StarType StarType => Variant.StarType;
        public bool NewCodexEntry => Variant.NewCodexEntry;
        public bool Confirmed => Variant.Confirmed;
        public VariantChance Chance => Variant.Chance;
        public bool NewCodex => Variant.NewCodexEntry;
        public bool HasMoreVariants => NonConfirmed && vm.Variants.Count > vm.Variants.IndexOf(this) + 1;
        public bool NotConfirmed => vm.UnConfirmed || (NonConfirmed == false && Confirmed == false);
        public bool NonConfirmed
        {
            get
            {
                return !vm.Variants.Any(x => x.Confirmed);
            }
        }

        public bool ContainsVariant(OrganicVariant variantToCheck) => variantToCheck == this.Variant;

        internal void OnInfoUpdated()
        {
            OnPropertyChanged(nameof(NonConfirmed));
            OnPropertyChanged(nameof(NotConfirmed));
            OnPropertyChanged(nameof(HasMoreVariants));
            OnPropertyChanged(nameof(Confirmed));
        }
    }
}
