using ODExplorer.AppSettings.NoteableBody;
using ODExplorer.Utils;
using System.ComponentModel;

namespace ODExplorer.AppSettings
{
    public class SettingsValues : PropertyChangeNotify
    {
        private DisplaySettings displaySettings = new();
        public DisplaySettings DisplaySettings { get => displaySettings; set { displaySettings = value; OnPropertyChanged(); } }

        private Noteable noteableSettings = new();
        public Noteable NotableSettings { get => noteableSettings; set { noteableSettings = value; OnPropertyChanged(); } }

        private SortCategory sortCategory = SortCategory.Value;
        private ListSortDirection sortDirection = ListSortDirection.Descending;
        private int worthMappingValue = 300000;
        private int worthMappingDistance;
        private bool ignoreNonBodies = true;
        private bool autoCopyCsvSystemToClipboard = true;
        private bool autoSelectNextCsvSystem = true;
        private bool showParser;
        private Temperature temperatureUnit;
        private double uiScale = 1;
        private bool showAdditionWindowsInTaskBar;
        private bool excludeStarsFromSorting = true;
        private bool autoStartFleetCarrierTimer = true;

        public SortCategory SortCategory { get => sortCategory; set { sortCategory = value; OnPropertyChanged(); } }
        public ListSortDirection SortDirection { get => sortDirection; set { sortDirection = value; OnPropertyChanged(); } }
        public int WorthMappingValue { get => worthMappingValue; set { worthMappingValue = value; OnPropertyChanged(); } }
        public int WorthMappingDistance { get => worthMappingDistance; set { worthMappingDistance = value; OnPropertyChanged(); } }
        public bool IgnoreNonBodies { get => ignoreNonBodies; set { ignoreNonBodies = value; OnPropertyChanged(); } }
        public bool ShowParser { get => showParser; set { showParser = value; OnPropertyChanged(); } }
        public bool AutoCopyCsvSystemToClipboard { get => autoCopyCsvSystemToClipboard; set { autoCopyCsvSystemToClipboard = value; OnPropertyChanged(); } }
        public bool AutoSelectNextCsvSystem { get => autoSelectNextCsvSystem; set { autoSelectNextCsvSystem = value; OnPropertyChanged(); } }
        public Temperature TemperatureUnit { get => temperatureUnit; set { temperatureUnit = value; OnPropertyChanged(); } }
        public double UiScale { get => uiScale; set { uiScale = value; OnPropertyChanged(); } }
        public bool ShowAdditionalWindowsInTaskBar { get => showAdditionWindowsInTaskBar; set { showAdditionWindowsInTaskBar = value; OnPropertyChanged(); } }
        public bool ExcludeStarsFromSorting { get => excludeStarsFromSorting; set { excludeStarsFromSorting = value; OnPropertyChanged(); } }
        public bool AutoStartFleetCarrierTimer {  get => autoStartFleetCarrierTimer; set { autoStartFleetCarrierTimer = value; OnPropertyChanged(); } }
        public Theme CurrentTheme { get => Settings.CurrentTheme; set { Settings.CurrentTheme = value; OnPropertyChanged(); } }

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
            ExcludeStarsFromSorting = true;
        }
    }
}
