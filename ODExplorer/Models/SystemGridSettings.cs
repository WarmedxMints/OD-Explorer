using Newtonsoft.Json;
using System.ComponentModel;

namespace ODExplorer.Models
{
    public sealed class SystemGridSettings
    {
        public BodySortCategory BodySortingOptions { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public BodyInfoIconDisplay InfoDisplayOptions { get; set; }
        public Temperature TemperatureDisplay { get; set; }
        public int ValuableBodyValue { get; set; }
        public int ValuableBodyDistance { get; set; }
        public bool IgnoreNonBodies { get; set; }
        public bool ExcludeStarsFromSorting { get; set; }
        public bool ShowBodyHeaders { get; set; }
        public Pressure PressureUnit { get; set; }
        public Distance DistanceUnit { get; set; }
        public int ExoValuableBodyValue { get; set; } = 20_000_000;
        public int MinExoValue { get; set; } = 0;
        public bool FilterUnconfirmedBios { get; set; } = true;
        public bool ShowBodyIcon { get; set; } = true;
        public bool ShowBodyId { get; set; } = true;
        public static SystemGridSettings DefaultValues()
        {
            return new()
            {
                BodySortingOptions = BodySortCategory.WorthMappingDistance,
                SortDirection = ListSortDirection.Ascending,
                InfoDisplayOptions = BodyInfoIconDisplay.All,
                TemperatureDisplay = Temperature.Kelvin,
                ValuableBodyValue = 500_000,
                ValuableBodyDistance = 0,
                ExoValuableBodyValue = 20_000_000,
                MinExoValue = 0,
                IgnoreNonBodies = true,
                ExcludeStarsFromSorting = true,
                PressureUnit = Pressure.Pascal,
                DistanceUnit = Distance.Kilometers
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not SystemGridSettings setting)
                return false;

            return BodySortingOptions.Equals(setting.BodySortingOptions)
                && SortDirection.Equals(setting.SortDirection)
                && InfoDisplayOptions.Equals(setting.InfoDisplayOptions)
                && TemperatureDisplay.Equals(setting.TemperatureDisplay)
                && ValuableBodyValue == setting.ValuableBodyValue
                && ValuableBodyDistance == setting.ValuableBodyDistance
                && IgnoreNonBodies == setting.IgnoreNonBodies
                && ExcludeStarsFromSorting == setting.ExcludeStarsFromSorting
                && ShowBodyHeaders == setting.ShowBodyHeaders
                && PressureUnit == setting.PressureUnit
                && DistanceUnit == setting.DistanceUnit
                && ExoValuableBodyValue == setting.ExoValuableBodyValue
                && MinExoValue == setting.MinExoValue
                && FilterUnconfirmedBios == setting.FilterUnconfirmedBios
                && ShowBodyIcon == setting.ShowBodyIcon
                && ShowBodyId == setting.ShowBodyId;
        }

        public SystemGridSettings Clone()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.None);
            var clone = JsonConvert.DeserializeObject<SystemGridSettings>(json);
            if (clone != null)
            {
                return clone;
            }
            return this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
