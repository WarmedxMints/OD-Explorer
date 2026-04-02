using EliteJournalReader;
using ODExplorer.Extensions;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Exobiology;
using ODUtils.Extensions;
using ODUtils.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class OrganicScanItemViewModel(OrganicScanItem item) : OdViewModelBase
    {
        public OrganicScanItem Item => item;
        public string GenusCodex => Item.GenusCodex;
        public string GenusEnglish
        {
            get
            {
                if (string.IsNullOrEmpty(Item.GenusEnglish))
                    return item.GenusLocalised;

                return Item.GenusEnglish;
            }
        }

        public string SpeciesEnglish
        {
            get
            {
                if (Item is null || string.IsNullOrEmpty(Item.SpeciesEnglish))
                    return item.SpeciesLocalised;

                return Item.SpeciesEnglish;
            }
        }

        public ObservableCollection<OrganicVariantViewModel> Variants { get; set; } = [];
        public string SystemName => Item.Body.Owner.Name;
        public string BodyName => Item.Body.BodyName;
        public string ScanStage => Item.ScanStage.GetEnumDescription();
        public bool WasLogged => Item.WasLogged;
        public OrganicScanStage ScanStageEnum
        {
            get
            {
                return Item.ScanStage;
            }
        }

        public string Colour
        {
            get
            {
                var variant = Item.Variants.FirstOrDefault(x => x.Confirmed);
                if (variant is null || variant?.Colour == VariantColours.Unknown)
                {
                    return string.Empty;
                }
                return $"{variant?.Colour}";
            }
        }
        public string EnglishName
        {
            get
            {
               
                var variant = Item.Variants.FirstOrDefault(x => x.Confirmed);
                if(variant is null || variant?.Colour == VariantColours.Unknown)
                {
                    return item.SpeciesLocalised;
                }
                return $"{item.GenusEnglish} {item.SpeciesEnglish} - {variant?.Colour}";
            }
        }
        public string ValueString => Info?.Value.FormatNumber() ?? string.Empty;
        public long Value => Info?.Value ?? 0;
        public string Region => Item.Body.Owner.Region.Name;
        public string BonusString => Bonus.FormatNumber();
        public long Bonus => Item.Body.WasMapped ? 0 : Item.ScanTime < OrganicValues.NewPriceDate ? Value : Value * 4;
        public string EstimatedValue => Item.TotalValue == 0 ? string.Empty : Item.TotalValue.ToString("N0");
        public OrganicInfoViewModel? Info => Item.Info is null ? null : new(Item.Info);
        public bool UnConfirmed => Item.ScanStage == OrganicScanStage.Prediction && Item.BodyDssScanned;
        public bool NewSpecies => Item.IsNewSpecies;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public string ColonyRange => SettingsStore.Instance?.SystemGridSetting.DistanceUnit switch
        {
            Distance.Miles => CheckInfo() is null ? string.Empty : $"{Item.Info.ColonyRange * 3.280839895:N0} ft",
            _ => CheckInfo() is null ? string.Empty : $"{Item.Info.ColonyRange:N0} m",
        };
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        private OrganicInfo? CheckInfo()
        {
            if (Item.Info is null || SpeciesEnglish == string.Empty)
                return null;

            return Item.Info;
        }
        public int AlternationIndex { get; set; }

        public bool IsHidden { get; set; }
        public void OnInfoUpdated()
        {
            if (Variants.Count == 0)
            {
                
                    var newOrganic = new OrganicScanItemViewModel(item);

                    foreach (var variant in item.Variants)
                    {
                        Variants.AddToCollection(new(variant, newOrganic));
                    }
            }
            OnPropertyChanged(nameof(SystemName));
            OnPropertyChanged(nameof(ColonyRange));
            OnPropertyChanged(nameof(EstimatedValue));
            OnPropertyChanged(nameof(BodyName));
            OnPropertyChanged(nameof(ScanStage));
            OnPropertyChanged(nameof(EnglishName));
            OnPropertyChanged(nameof(ValueString));
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(Region));
            OnPropertyChanged(nameof(BonusString));
            OnPropertyChanged(nameof(Bonus));
            OnPropertyChanged(nameof(Info));
            OnPropertyChanged(nameof(UnConfirmed));
            OnPropertyChanged(nameof(GenusEnglish));
            OnPropertyChanged(nameof(SpeciesEnglish));
            OnPropertyChanged(nameof(ScanStageEnum));
            OnPropertyChanged(nameof(Variants));
            OnPropertyChanged(nameof(AlternationIndex));
            OnPropertyChanged(nameof(NewSpecies));
            OnPropertyChanged(nameof(WasLogged));
            OnPropertyChanged(nameof(Colour));

            foreach (var variant in Variants)
            {
                variant.OnInfoUpdated();
            }
        }
    }
}
