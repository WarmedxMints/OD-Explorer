using EliteJournalReader;
using EliteJournalReader.Events;
using NAudio.Wave;
using ODExplorer.Database;
using ODExplorer.Models;
using ODUtils.Database.Interfaces;
using ODUtils.Helpers;
using ODUtils.Journal;
using ODUtils.Spansh;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Printing;
using System.Threading.Tasks;

namespace ODExplorer.Stores
{
    public sealed class SpanshCsvStore : IProcessJournalLogs
    {
        public SpanshCsvStore(JournalParserStore parserStore, IOdToolsDatabaseProvider odToolsDatabase, SettingsStore settingsStore, NotificationStore notificationStore)
        {
            this.parserStore = parserStore;
            this.settingsStore = settingsStore;
            this.notificationStore = notificationStore;

            databaseProvider = (OdExplorerDatabaseProvider)odToolsDatabase;

            parserStore.RegisterParser(this);
            parserStore.OnParserStoreLive += ParserStore_OnParserStoreLive;

            fleetCarrierTimer.CountDownFinishedEvent += FleetCarrierTimer_CountDownFinishedEvent;
        }

        private readonly JournalParserStore parserStore;
        private readonly SettingsStore settingsStore;
        private readonly NotificationStore notificationStore;
        private readonly OdExplorerDatabaseProvider databaseProvider;
        private readonly CountdownTimer fleetCarrierTimer = new(new(0, 20, 0));
        private Dictionary<CsvType, SpanshCsvContainer> containers = [];
        private CsvType currentContainerType;
        private string carrierName = string.Empty;

        public event EventHandler<ExplorationTarget?>? OnCurrentTargetChanged;
        public event EventHandler<SpanshCsvContainer?>? OnCurrentContainerChanged;

        public event EventHandler<string>? OnCarrierTimeTick
        {
            add { fleetCarrierTimer.OnTick += value; }
            remove { fleetCarrierTimer.OnTick -= value; }
        }

        public event EventHandler<bool>? OnCarrierTimerRunning
        {
            add { fleetCarrierTimer.OnTimerRunning += value; }
            remove { fleetCarrierTimer.OnTimerRunning -= value; }
        }

        public event EventHandler? OnCarrierTimerFinished
        {
            add { fleetCarrierTimer.CountDownFinishedEvent += value; }
            remove { fleetCarrierTimer.CountDownFinishedEvent -= value; }
        }

        public bool CarrierTimerRunning => fleetCarrierTimer.TimerRunning;
        public SpanshCsvContainer? CurrentContainer { get; private set; }

        private ExplorationTarget? currentTarget;
        public ExplorationTarget? CurrentTarget
        {
            get => currentTarget;

            private set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
                    OnCurrentTargetChanged?.Invoke(this, currentTarget);
                }
            }
        }

        public ExplorationTarget? NextTarget
        {
            get
            {
                if (CurrentContainer is null || CurrentContainer.Targets.Count == 0 || CurrentContainer.CurrentIndex > CurrentContainer.Targets.Count - 2)
                {
                    return null;
                }

                return CurrentContainer.Targets[CurrentContainer.CurrentIndex + 1];
            }
        }
        public int CurrentIndex
        {
            get
            {
                if (CurrentContainer == null)
                    return -1;

                return CurrentContainer.CurrentIndex;
            }
            set
            {
                if (CurrentContainer == null)
                {

                    return;
                }

                var targets = CurrentContainer.Targets;

                if (targets == null || targets.Count == 0)
                {
                    CurrentContainer.CurrentIndex = 0;
                    return;
                }

                CurrentContainer.CurrentIndex = Math.Clamp(value, 0, targets.Count - 1);

                CurrentTarget = targets[CurrentContainer.CurrentIndex];
            }
        }

        public SpanshCsvContainer SetCurrentCSVContainer(CsvType csvType)
        {
            CurrentTarget = null;
            settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID] = csvType;
            var ret = GetCurrentContainer(csvType);
            OnCurrentContainerChanged?.Invoke(this, ret);
            return ret;
        }

        public SpanshCsvContainer GetCurrentContainer(CsvType csvType)
        {
            currentContainerType = csvType;

            if (containers.TryGetValue(csvType, out var container))
            {
                CurrentContainer = container;
                CurrentIndex = CurrentContainer.CurrentIndex;
                return container;
            }

            var ret = new SpanshCsvContainer([], 0);
            CurrentIndex = -1;
            CurrentContainer = ret;
            return ret;
        }

        public void SaveCSVs()
        {
            var toDave = containers.Where(x => x.Value.HasValue).ToDictionary();

            databaseProvider.SaveCVSs(toDave, settingsStore.SelectedCommanderID);
        }

        #region IProcessJournalLogs Implementation
        public void ClearData()
        {
            containers.Clear();
            CurrentContainer = null;
            CurrentIndex = -1;
            CurrentTarget = null;
        }

        public void Dispose()
        {
            parserStore.UnregisterParser(this);
        }

        public JournalHistoryArgs GetEventsToParse(DateTime defaultAge) => new([JournalTypeEnum.CarrierStats, JournalTypeEnum.CarrierJumpRequest, JournalTypeEnum.CarrierLocation, JournalTypeEnum.CarrierJumpCancelled], defaultAge, this, ParseHistoryStream);

        public void ParseHistory(IEnumerable<JournalEntry> journalEntries, int currentCmdrId) { }

        public Task ParseHistoryStream(JournalEntry entry)
        {
            ParseJournalEvent(entry);
            return Task.CompletedTask;
        }

        private bool CheckCarrierType(string type, DateTime date)
        {
            if (date <= PatchDates.SquadCarrierPatchDate)
                return false;

            return string.Equals(type, "FleetCarrier", StringComparison.OrdinalIgnoreCase) == false;
        }

        public void ParseJournalEvent(JournalEntry evt)
        {
            switch (evt.EventData)
            {
                case CarrierStatsEvent.CarrierStatsEventArgs carrierStats:
                    if (CheckCarrierType(carrierStats.CarrierType, carrierStats.Timestamp))
                        break;

                    if (string.IsNullOrEmpty(carrierStats.Name) == false)
                    {
                        carrierName = carrierStats.Name;
                        break;
                    }
                    carrierName = carrierStats.Callsign;
                    break;
                case CarrierJumpRequestEvent.CarrierJumpRequestEventArgs carrierJumpRequest:
                    if (CheckCarrierType(carrierJumpRequest.CarrierType, carrierJumpRequest.Timestamp))
                        break;
                    var timespan = (carrierJumpRequest.DepartureTime - DateTime.UtcNow) + TimeSpan.FromMinutes(5);

                    if (timespan > TimeSpan.Zero)
                    {
                        StartTimer(timespan.Hours, timespan.Minutes, timespan.Seconds);
                    }
                    break;
                case CarrierJumpCancelledEvent.CarrierJumpCancelledEventArgs carrierJumpCancelled:
                    if (CheckCarrierType(carrierJumpCancelled.CarrierType, carrierJumpCancelled.Timestamp))
                        break;
                    StopFleetCarrierTimer();
                    break;
                case CarrierLocationEvent.CarrierLocationEventArgs carrierLocation:
                    if (CheckCarrierType(carrierLocation.CarrierType, carrierLocation.Timestamp))
                        break;
                    OnCurrentSystemChanged(carrierLocation.StarSystem);
                    break;
                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    OnCurrentSystemChanged(fsdJump.StarSystem);
                    break;
                case CarrierJumpEvent.CarrierJumpEventArgs carrierJump:
                    OnCurrentSystemChanged(carrierJump.StarSystem);
                    break;
            }
        }

        public void RunBeforeParsingLogs(int currentCmdrId) { }
        #endregion

        private void ParserStore_OnParserStoreLive(object? sender, bool e)
        {
            if (e)
            {
                _ = Task.Factory.StartNew(() =>
                {
                    containers = databaseProvider.GetSpanshCSVsDictionary(settingsStore.SelectedCommanderID);
                    SetCurrentCSVContainer(settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID]);
                });
            }
        }

        private void OnCurrentSystemChanged(string? system)
        {
            if (parserStore.IsLive == false || system is null || CurrentContainer is null || CurrentContainer.Targets.Count < 1)
            {
                return;
            }

            if (NextTarget is not null && NextTarget.SystemName.Equals(system, StringComparison.OrdinalIgnoreCase))
            {
                int index = CurrentContainer.Targets.IndexOf(NextTarget);

                if (index > CurrentIndex)
                {
                    CurrentIndex = index;

                    if (currentContainerType == CsvType.GalaxyPlotter
                    && string.IsNullOrEmpty(CurrentTarget?.Property3) == false
                    && CurrentTarget.Property3.Contains("Yes", StringComparison.OrdinalIgnoreCase))
                    {
                        notificationStore.ShowSpanshNotification(Notifications.SpanshNotificationType.Refuel);
                    }

                    if (NextTarget != null && settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard)
                    {
                        notificationStore.CopyToClipBoard(NextTarget.SystemName);
                    }
                }
            }
        }

        public bool ParseCSV(string filename)
        {
            var csv = SpanshCSVParser.ParseCsv(filename);

            if (csv is null)
            {
                return false;
            }

            if (containers.ContainsKey(csv.CsvType))
            {
                containers[csv.CsvType] = new(csv.Targets, 0);
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            if (containers.TryAdd(csv.CsvType, new(csv.Targets, 0)))
            {
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            return false;
        }

        public bool ForceParseCSV(string filename, CsvType csvType)
        {
            var csv = SpanshCSVParser.ForceParse(filename, csvType);

            if (csv is null)
            {
                return false;
            }

            if (containers.ContainsKey(csv.CsvType))
            {
                containers[csv.CsvType] = new(csv.Targets, 0);
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            if (containers.TryAdd(csv.CsvType, new(csv.Targets, 0)))
            {
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            return false;
        }

        public void StartFleetCarrierTimer()
        {
            StopFleetCarrierTimer();
            StartTimer();
        }

        public void StopFleetCarrierTimer()
        {
            if (fleetCarrierTimer.TimerRunning)
            {
                fleetCarrierTimer.Stop();
            }
        }

        public void StartTimer(int hours = 0, int minutes = 20, int seconds = 0)
        {
            fleetCarrierTimer.UpdateRuntime(new TimeSpan(hours, minutes, seconds));
            fleetCarrierTimer.Start();
        }

        private void FleetCarrierTimer_CountDownFinishedEvent(object? sender, EventArgs e)
        {
            if (settingsStore.SpanshCSVSettings.NotifyOnFcTimerCompletion)
            {
                notificationStore.FleetCarrierNotification(carrierName);
            }
            _ = Task.Factory.StartNew(() =>
            {
                if (string.IsNullOrEmpty(settingsStore.SpanshCSVSettings.CustomTimerSound) == false
                    && File.Exists(settingsStore.SpanshCSVSettings.CustomTimerSound))
                {
                    try
                    {
                        using var audioFile = new AudioFileReader(settingsStore.SpanshCSVSettings.CustomTimerSound);
                        using var outputDevice = new WaveOutEvent();
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                        { }
                        return;
                    }
                    catch
                    {
                        SystemSounds.Beep.Play();
                        return;
                    }
                }

                SystemSounds.Beep.Play();
            });
        }
    }
}
