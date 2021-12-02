using LoadSaveSystem;
using ODExplorer.Utils;
using System.ComponentModel;
using System.IO;

namespace ODExplorer.AppSettings
{
    public enum SortCategory
    {
        [Description("Mapped Value")]
        Value,
        [Description("Surface Gravity")]
        Gravity,
        [Description("Distance From Arrival")]
        Distance,
        [Description("Body Type")]
        Type,
        [Description("Body Name")]
        Name,
        [Description("Body ID")]
        BodyId,
        [Description("Biological Signals")]
        BioSignals,
        [Description("Geological Signals")]
        GeoSignals,
        [Description("Worth Mapping/Value")]
        WorthMappingValue,
        [Description("Worth Mapping/Distance")]
        WorthMappingDistance,
        [Description("No Sorting")]
        None
    }

    public enum Temperature
    {
        Kelvin,
        Celsius,
        Fahrenheit
    }

    public class Settings
    {
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "Settings.json");

        public static Settings SettingsInstance { get; private set; }
        public Settings()
        {
            if (SettingsInstance is not null)
            {
                return;
            }

            SettingsInstance = this;

            Value = LoadSave.LoadJson<SettingsValues>(_saveFile);

            if (Value is null)
            {
                Value = new();
            }
        }

        public SettingsValues Value { get; set; }

        public bool SaveSettings()
        {
            return LoadSave.SaveJson(Value, _saveFile);
        }
    }

    public class SettingsValues : PropertyChangeNotify
    {
        private SortCategory sortCategory = SortCategory.Value;
        private ListSortDirection sortDirection = ListSortDirection.Descending;
        private int worthMappingValue = 300000;
        private int worthMappingDistance;
        private bool ignoreNonBodies = true;
        private bool autoCopyCsvSystemToClipboard = true;
        private bool showParser;
        private Temperature temperatureUnit;
        private double uiScale = 1;

        public SortCategory SortCategory { get => sortCategory; set { sortCategory = value; OnPropertyChanged(); } }
        public ListSortDirection SortDirection { get => sortDirection; set { sortDirection = value; OnPropertyChanged(); } }
        public int WorthMappingValue { get => worthMappingValue; set { worthMappingValue = value; OnPropertyChanged(); } }
        public int WorthMappingDistance { get => worthMappingDistance; set { worthMappingDistance = value; OnPropertyChanged(); } }
        public bool IgnoreNonBodies { get => ignoreNonBodies; set { ignoreNonBodies = value; OnPropertyChanged(); } }
        public bool ShowParser { get => showParser; set { showParser = value; OnPropertyChanged(); } }
        public bool AutoCopyCsvSystemToClipboard { get => autoCopyCsvSystemToClipboard; set { autoCopyCsvSystemToClipboard = value; OnPropertyChanged(); } }
        public Temperature TemperatureUnit { get => temperatureUnit; set { temperatureUnit = value; OnPropertyChanged(); } }
        public double UiScale { get => uiScale; set { uiScale = value; OnPropertyChanged(); } }

        public void Copy(SettingsValues values)
        {
            SortCategory = values.SortCategory;
            SortDirection = values.SortDirection;
            WorthMappingValue = values.WorthMappingValue;
            WorthMappingDistance = values.WorthMappingDistance;
            IgnoreNonBodies = values.IgnoreNonBodies;
            ShowParser = values.ShowParser;
            AutoCopyCsvSystemToClipboard = values.AutoCopyCsvSystemToClipboard;
            TemperatureUnit = values.TemperatureUnit;
            UiScale = values.UiScale;
        }

        public void Reset()
        {
            SortCategory = SortCategory.Value;
            SortDirection = ListSortDirection.Descending;
            WorthMappingValue = 300000;
            WorthMappingDistance = 0;
            IgnoreNonBodies = true;
            AutoCopyCsvSystemToClipboard = true;
            ShowParser = false;
            TemperatureUnit = Temperature.Kelvin;
            UiScale = 1;
        }
    }
}
