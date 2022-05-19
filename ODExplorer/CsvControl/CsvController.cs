using LoadSaveSystem;
using ODExplorer.Utils;
using ParserLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ODExplorer.CsvControl
{
    public class CsvController : PropertyChangeNotify
    {
        private readonly string _previousSession = Path.Combine(Directory.GetCurrentDirectory(), "Data", "CSVSessions.json");

        public CsvController()
        {
            int count = Enum.GetNames(typeof(CsvType)).Length;

            for (int i = 0; i < count; i++)
            {
                CsvContainers.Add(new CsvContainer());
            }

            AppSettings.Settings.SettingsInstance.SaveEvent += SettingsInstance_SaveEvent;
        }

        ~CsvController()
        {
            AppSettings.Settings.SettingsInstance.SaveEvent -= SettingsInstance_SaveEvent;
        }

        private ObservableCollection<CsvContainer> csvContainers = new();
        public ObservableCollection<CsvContainer> CsvContainers { get => csvContainers; set { csvContainers = value; OnPropertyChanged(); } }

        public ExplorationTarget CurrentTarget
        {
            get => CsvContainers[(int)CurrentCsvType].CurrentTarget;
            set
            {
                CsvContainers[(int)currentCsvType].CurrentTarget = value;

                OnCurrentTargetUpdated?.Invoke(this, new CsvEventArgs { CsvType = CurrentCsvType, Target = value });
                UpdateUiProperties();
            }
        }

        public event EventHandler<CsvEventArgs> OnCurrentTargetUpdated;
        public bool CurrentTargetHasData
        {
            get
            {
                if (CurrentTarget is null || string.IsNullOrEmpty(CurrentTarget.SystemName) || CurrentTarget.SystemName == "No Data")
                {
                    return false;
                }

                return true;
            }
        }
        private CsvType currentCsvType;
        public CsvType CurrentCsvType { get => currentCsvType; set { currentCsvType = value; OnPropertyChanged(); UpdateUiProperties(); } }

        public int CurrentIndex
        {
            get
            {
                int index = (int)CurrentCsvType;

                return index < 0 ? index : CsvContainers[index].CurrentIndex;
            }
            set
            {
                int index = (int)CurrentCsvType;
                List<ExplorationTarget> Targets = CsvContainers[index].Targets;

                if (Targets == null || Targets.Count < 1)
                {
                    CsvContainers[index].CurrentIndex = 0;
                    return;
                }

                CsvContainers[index].CurrentIndex = value;

                if (CsvContainers[index].CurrentIndex > Targets.Count - 1)
                {
                    CsvContainers[index].CurrentIndex = Targets.Count - 1;
                }
                else if (CsvContainers[index].CurrentIndex < 0)
                {
                    CsvContainers[index].CurrentIndex = 0;
                }

                CurrentTarget = Targets[CsvContainers[index].CurrentIndex];
            }
        }

        private void UpdateUiProperties()
        {
            OnPropertyChanged("PreviousSystems");
            OnPropertyChanged("NextSystems");
            OnPropertyChanged("RemainingCount");
            OnPropertyChanged("CurrentTargetHasData");
            OnPropertyChanged("CurrentTarget");
            OnPropertyChanged("CsvHeader");
        }

        public bool PreviousSystems
        {
            get
            {
                CsvContainer container = GetCsvContainer(CurrentCsvType);

                return container is not null && container.Targets != null && container.Targets.Count >= 1 && container.CurrentIndex > 0;
            }
        }

        public bool NextSystems
        {
            get
            {
                CsvContainer container = GetCsvContainer(CurrentCsvType);

                return container is not null && container.Targets != null && container.Targets.Count >= 1 && container.CurrentIndex < container.Targets.Count - 1;
            }
        }

        public int RemainingCount
        {
            get
            {
                CsvContainer container = GetCsvContainer(CurrentCsvType);

                return container is not null && container.Targets.Count > 0 ? container.Targets.Count - container.CurrentIndex - 1 : 0;
            }
        }

        private CsvContainer GetCsvContainer(CsvType csvType)
        {
            int index = (int)csvType;

            return index > CsvContainers.Count - 1 ? null : CsvContainers[index];
        }

        public void ProcessCsv(CsvParserReturn csvArgs)
        {
            CsvContainer container = GetCsvContainer(csvArgs.CsvType);

            if (container is null)
            {
                return;
            }

            container.Targets = new List<ExplorationTarget>(csvArgs.Targets);
            CurrentCsvType = csvArgs.CsvType;
            container.CurrentIndex = 0;
            CurrentIndex = 0;
        }

        internal void NavData_OnCurrentSystemChanged(NavData.SystemInfo systemInfo)
        {
            CsvContainer container = GetCsvContainer(CurrentCsvType);

            if (container is null || container.Targets.Count < 1)
            {
                return;
            }

            ExplorationTarget target = container.Targets.FirstOrDefault(x => x.SystemName.Equals(systemInfo.SystemName, StringComparison.OrdinalIgnoreCase));

            if (target is not null)
            {
                int index = container.Targets.IndexOf(target);

                if (index >= CurrentIndex)
                {
                    CurrentIndex = index + 1;
                }
            }
        }

        public string[] CsvHeader
        {
            get
            {
                return CurrentCsvType switch
                {
                    CsvType.RoadToRiches => new string[] { "DISTANCE", "VALUE" },
                    CsvType.WorldTypeRoute => new string[] { "DISTANCE", "JUMPS" },
                    CsvType.Exobiology => new string[] {"GENUS", "VALUE"},
                    _ => null,
                };
            }
        }

        public bool LoadPreviousSession()
        {
            if (!File.Exists(_previousSession))
            {
                return false;
            }

            CsvSave prevSession = LoadSave.LoadJson<CsvSave>(_previousSession);

            if (prevSession == null)
            {
                return false;
            }

            for (int i = 0; i < prevSession.Containers.Count; i++)
            {
                CsvContainers[i] = prevSession.Containers[i];
            }

            foreach (CsvContainer container in CsvContainers)
            {
                container.CurrentTarget = container.Targets.Count > 0 && container.Targets.Count > container.CurrentIndex ? container.Targets[container.CurrentIndex] : new();
            }

            CurrentCsvType = prevSession.CurrentCsvType;

            CsvContainer currentContainer = GetCsvContainer(CurrentCsvType);

            CurrentIndex = currentContainer is null ? 0 : currentContainer.CurrentIndex;

            return true;
        }

        private void SettingsInstance_SaveEvent(object sender, EventArgs e)
        {
            SaveState();
        }

        public void SaveState()
        {
            CsvSave save = new(this);

            _ = LoadSave.SaveJson(save, _previousSession);
        }
    }
}
