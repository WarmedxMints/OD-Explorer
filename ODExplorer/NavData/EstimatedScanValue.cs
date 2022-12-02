using LoadSaveSystem;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ODExplorer.NavData
{
    public class EstimatedScanValue : PropertyChangeNotify
    {
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ScanValue.json");

        public ObservableCollection<SystemInfo> ScannedSystems { get; set; } = new();

        public EstimatedScanValue()
        {
            List<SystemInfo> sys = LoadSave.LoadJson<List<SystemInfo>>(_saveFile);

            if (sys is not null)
            {
                foreach (SystemInfo system in sys)
                {
                    UpdateMainStarValue(system);
                }
            }
            AppSettings.Settings.SettingsInstance.SaveEvent += SettingsInstance_SaveEvent;
        }

        ~EstimatedScanValue()
        {
            AppSettings.Settings.SettingsInstance.SaveEvent -= SettingsInstance_SaveEvent;
        }

        private void SettingsInstance_SaveEvent(object sender, EventArgs e)
        {
            _ = SaveState();
        }

        private ulong estimatedScanValueMin;
        public ulong EstimatedScanValueMin { get => estimatedScanValueMin; private set { estimatedScanValueMin = value; OnPropertyChanged(); } }

        private ulong estimatedScanValueMax;
        public ulong EstimatedScanValueMax { get => estimatedScanValueMax; private set { estimatedScanValueMax = value; OnPropertyChanged(); } }

        public void UpdateMainStarValue(SystemInfo systemInfo)
        {
            if (!ScannedSystems.Contains(systemInfo))
            {
                ScannedSystems.AddToCollection(systemInfo);
            }

            //Find main star
            SystemBody star = systemInfo.Bodies.FirstOrDefault(x => x.DistanceFromArrivalLs <= 0.01 && x.IsStar);

            //If we haven't scanned it, return
            if (star is null)
            {
                UpdatesEstimatedScanValue();
                return;
            }

            int[] values = CalcSystemFssValues(systemInfo, star);

            //Calc the main star bonus
            double StellaBonus = values[0] > 0 ? (values[0] / 3.0) : 0;
            double PlanetBonus = values[1] > 0 ? Math.Max(500, values[1] / 3.0) : 0;

            star.BonusValue = (int)(PlanetBonus + StellaBonus);
            star.MappedValue = star.FssValue + star.BonusValue;

            UpdatesEstimatedScanValue();
        }

        private int[] CalcSystemFssValues(SystemInfo system, SystemBody mainStar)
        {
            int[] val = new int[2] { 0, 0 };

            foreach (SystemBody body in system.Bodies)
            {
                if (body == mainStar)
                {
                    continue;
                }
                if (body.IsStar)
                {
                    val[0] += body.FssValue;
                    continue;
                }
                if (body.IsPlanet)
                {
                    val[1] += body.FssValue;
                }
            }
            return val;
        }

        public void UpdatesEstimatedScanValue()
        {
            if (!ScannedSystems.Any())
            {
                estimatedScanValueMin = 0;
                EstimatedScanValueMax = 0;
                return;
            }

            ulong val = 0;
            ulong valMin = 0;
            foreach (SystemInfo system in ScannedSystems)
            {
                if(system.DataSold)
                {
                    continue;
                }

                foreach (SystemBody body in system.Bodies)
                {
                    if (body.MappedByUser)
                    {
                        valMin += (ulong)MathFunctions.GetPlanetValue(body.PlanetClass, body.MassEM, !body.WasDiscovered, !body.Wasmapped, false, NavigationData.Odyssey, true, body.EffeicentMapped);
                        val += (ulong)(body.MappedValue + body.BonusValue);
                        continue;
                    }
                    val += (ulong)(body.FssValue + body.BonusValue);
                    valMin += (ulong)(body.FssValue + body.BonusValue);
                }
            }

            EstimatedScanValueMin = valMin;
            EstimatedScanValueMax = val;
        }

        public void SellExplorationData(EliteJournalReader.Events.SellExplorationDataEvent.SellExplorationDataEventArgs args)
        {
            List<SystemInfo> systemsToRemove = new();

            foreach (string system in args.Systems)
            {
                foreach (SystemInfo sys in ScannedSystems)
                {
                    if (string.Equals(sys.SystemName, system, StringComparison.OrdinalIgnoreCase))
                    {
                        systemsToRemove.Add(sys);
                    }
                }
            }

            RemoveSystems(systemsToRemove, AppSettings.Settings.SettingsInstance.Value.DeleteSystemDataOnsale);

            _ = SaveState();
            UpdatesEstimatedScanValue();
        }

        public void SellExplorationData(EliteJournalReader.Events.MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs args)
        {
            List<SystemInfo> systemsToRemove = new();

            foreach (EliteJournalReader.SystemScan system in args.Discovered)
            {
                foreach (SystemInfo sys in ScannedSystems)
                {
                    if (string.Equals(sys.SystemName, system.SystemName, StringComparison.OrdinalIgnoreCase))
                    {
                        systemsToRemove.Add(sys);
                    }
                }
            }

            RemoveSystems(systemsToRemove, AppSettings.Settings.SettingsInstance.Value.DeleteSystemDataOnsale);

            _ = SaveState();
            UpdatesEstimatedScanValue();
        }

        private void RemoveSystems(List<SystemInfo> systemsToRemove, bool deleteSystemDataOnsale)
        {
            foreach (SystemInfo sysToRemove in systemsToRemove)
            {
                sysToRemove.DataSold = true;

                if (deleteSystemDataOnsale)
                {
                    ScannedSystems.RemoveFromCollection(sysToRemove);
                }
            }
        }


        public void Reset()
        {
            ScannedSystems.ClearCollection();
            _ = SaveState();
            EstimatedScanValueMin = 0;
            EstimatedScanValueMax = 0;
        }

        public bool SaveState()
        {
            return LoadSave.SaveJson(ScannedSystems, _saveFile);
        }
    }
}
