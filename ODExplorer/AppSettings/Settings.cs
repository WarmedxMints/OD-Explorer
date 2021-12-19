using LoadSaveSystem;
using ODExplorer.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace ODExplorer.AppSettings
{
    public enum Theme
    {
        [Description("pack://application:,,,/ODExplorer;component/Themes/DefaultTheme.xaml")]
        ODExplorer,
        [Description("pack://application:,,,/ODExplorer;component/Themes/Light.xaml")]
        Light,
        [Description("pack://application:,,,/ODExplorer;component/Themes/GreenTheme.xaml")]
        LoudGreen,
    }

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

    public class Settings : PropertyChangeNotify
    {
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "Settings.json");

        private static Theme _currentTheme = Theme.ODExplorer;

        public static Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                (Application.Current as App).ChangeSkin();
            }
        }

        public event EventHandler SaveEvent;
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

        private SettingsValues _value;
        public SettingsValues Value { get => _value; set { _value = value; OnPropertyChanged(); } }

        public SettingsValues ClonedValues { get; private set; }

        public void CloneValues()
        {
            ClonedValues = Value.Clone();
        }

        public void SetClonedValues()
        {
            SettingsValues clonedValues = ClonedValues.Clone();

            Value = clonedValues;
            //To prompt the onpropertyupdated for ui update
            Value.DisplaySettings = clonedValues.DisplaySettings;

            ClonedValues = null;

            _ = SaveSettings();
        }

        public void SaveAll()
        {
            SaveEvent?.Invoke(this, EventArgs.Empty);
        }

        public bool SaveSettings()
        {
            return LoadSave.SaveJson(Value, _saveFile);
        }
    }

    public class SettingsValues : PropertyChangeNotify
    {
        private DisplaySettings displaySettings = new();
        public DisplaySettings DisplaySettings { get => displaySettings; set { displaySettings = value; OnPropertyChanged(); } }

        private SortCategory sortCategory = SortCategory.Value;
        private ListSortDirection sortDirection = ListSortDirection.Descending;
        private int worthMappingValue = 300000;
        private int worthMappingDistance;
        private bool ignoreNonBodies = true;
        private bool autoCopyCsvSystemToClipboard = true;
        private bool showParser;
        private Temperature temperatureUnit;
        private double uiScale = 1;
        private bool showAdditionWindowsInTaskBar;
        private bool excludeStarsFromSorting = true;
        public SortCategory SortCategory { get => sortCategory; set { sortCategory = value; OnPropertyChanged(); } }
        public ListSortDirection SortDirection { get => sortDirection; set { sortDirection = value; OnPropertyChanged(); } }
        public int WorthMappingValue { get => worthMappingValue; set { worthMappingValue = value; OnPropertyChanged(); } }
        public int WorthMappingDistance { get => worthMappingDistance; set { worthMappingDistance = value; OnPropertyChanged(); } }
        public bool IgnoreNonBodies { get => ignoreNonBodies; set { ignoreNonBodies = value; OnPropertyChanged(); } }
        public bool ShowParser { get => showParser; set { showParser = value; OnPropertyChanged(); } }
        public bool AutoCopyCsvSystemToClipboard { get => autoCopyCsvSystemToClipboard; set { autoCopyCsvSystemToClipboard = value; OnPropertyChanged(); } }
        public Temperature TemperatureUnit { get => temperatureUnit; set { temperatureUnit = value; OnPropertyChanged(); } }
        public double UiScale { get => uiScale; set { uiScale = value; OnPropertyChanged(); } }
        public bool ShowAdditionalWindowsInTaskBar { get => showAdditionWindowsInTaskBar; set { showAdditionWindowsInTaskBar = value; OnPropertyChanged(); } }
        public bool ExludeStarsFromSorting { get => excludeStarsFromSorting; set => excludeStarsFromSorting = value; }
        public Theme CurrentTheme { get => Settings.CurrentTheme; set { Settings.CurrentTheme = value; OnPropertyChanged(); } }
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
