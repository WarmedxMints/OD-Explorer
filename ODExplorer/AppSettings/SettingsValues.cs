using Microsoft.Win32;
using ODExplorer.AppSettings.NoteableBody;
using ODExplorer.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace ODExplorer.AppSettings
{
    public class SettingsValues : PropertyChangeNotify
    {
        private WindowPos lastWindowPos = new();
        public WindowPos LastWindowPos { get => lastWindowPos; set { lastWindowPos = value; OnPropertyChanged(); } }

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

        private string defaultJournalPath = null;

        private string customJournalPath;

        public string CustomJournalPath { get => customJournalPath; set { customJournalPath = value; OnPropertyChanged(); } }

        [IgnoreDataMember]
        public string JournalPath
        {
            get
            {
                if (string.IsNullOrEmpty(CustomJournalPath))
                {
                    return GetJournalPath();
                }

                return CustomJournalPath;
            }
        }

        private string GetJournalPath()
        {
            if (string.IsNullOrEmpty(defaultJournalPath) == false)
            {
                return defaultJournalPath;
            }

            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Saved Games");
            var regKey = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Shell Folders";
            var regKeyValue = "{4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4}";
            string regValue = (string)Registry.GetValue(regKey, regKeyValue, defaultPath);

            if (string.IsNullOrEmpty(regValue))
            {
                defaultJournalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "Saved Games",
                        "Frontier Developments",
                        "Elite Dangerous");

                return defaultJournalPath;
            }

            defaultJournalPath = Path.Combine(regValue, "Frontier Developments", "Elite Dangerous");

            return defaultJournalPath;
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
            ExcludeStarsFromSorting = true;
        }
    }
}
