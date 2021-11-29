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
        public SortCategory SortCategory { get => sortCategory; set { sortCategory = value; OnPropertyChanged(); } }
        public ListSortDirection SortDirection { get => sortDirection; set { sortDirection = value; OnPropertyChanged(); } }
        public int WorthMappingValue { get => worthMappingValue; set { worthMappingValue = value; OnPropertyChanged(); } }
        public int WorthMappingDistance { get => worthMappingDistance; set { worthMappingDistance = value; OnPropertyChanged(); } }
        public bool IgnoreNonBodies { get => ignoreNonBodies; set { ignoreNonBodies = value; OnPropertyChanged(); } }
        public bool ShowParser { get => showParser; set { showParser = value; OnPropertyChanged(); } }
        public bool AutoCopyCsvSystemToClipboard { get => autoCopyCsvSystemToClipboard; set { autoCopyCsvSystemToClipboard = value; OnPropertyChanged(); } }

        public void Copy(SettingsValues values)
        {
            sortCategory = values.sortCategory;
            sortDirection = values.sortDirection;
            worthMappingValue = values.worthMappingValue;
            worthMappingDistance = values.worthMappingDistance;
            IgnoreNonBodies = values.IgnoreNonBodies;
            ShowParser = values.showParser;
            AutoCopyCsvSystemToClipboard = values.autoCopyCsvSystemToClipboard;
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
        }
    }
}
