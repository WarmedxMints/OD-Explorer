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
        [Description("pack://application:,,,/ODExplorer;component/Themes/BlackAndWhite.xaml")]
        BlackAndWhite,
        [Description("pack://application:,,,/ODExplorer;component/Themes/GreenTheme.xaml")]
        Green,
        [Description("pack://application:,,,/ODExplorer;component/Themes/CustomTheme.xaml")]
        Custom,
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
            ClonedValues.NotableSettings.Atmospheres.UpdateMenuItems();
            ClonedValues.NotableSettings.Volcanism.UpdateMenuItems();
            ClonedValues.NotableSettings.NoteablePresets.BuildMenu();
        }

        public void SetClonedValues()
        {
            SettingsValues clonedValues = ClonedValues.Clone();

            Value = clonedValues;
            //To prompt the onpropertyupdated for ui update
            Value.DisplaySettings = clonedValues.DisplaySettings;
            Value.NotableSettings = clonedValues.NotableSettings;

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
}
