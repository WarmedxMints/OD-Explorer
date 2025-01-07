using EliteJournalReader.Events;
using ODExplorer.Database;
using ODUtils.Database.Interfaces;
using ODUtils.Journal;
using ODUtils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODExplorer.Stores
{
    public sealed class JournalParserStore
    {
        public JournalParserStore(JournalEventParser journalEventParser,
                                         IOdToolsDatabaseProvider odToolsDatabase,
                                         SettingsStore settingsStore)
        {
            this.journalEventParser = journalEventParser;
            this.odToolsDatabase = odToolsDatabase;
            this.settingsStore = settingsStore;

            journalEventParser.OnJournalEventReceived += OnJournalEventReceived;
            journalEventParser.LiveStatusChange += async (sender, e) => await OnJournalLiveStatusChange(e);
            journalEventParser.OnReadingNewFile += OnReadingNewJournalFile;
        }

        private readonly JournalEventParser journalEventParser;
        private readonly IOdToolsDatabaseProvider odToolsDatabase;
        private readonly SettingsStore settingsStore;
        private readonly List<IProcessJournalLogs> journalLogParserList = [];
        private readonly List<JournalCommander> _journalCommanders = [];

        #region Events
        public event EventHandler<string?>? OnJournalStoreStatusChange;
        public event EventHandler<bool>? OnParserStoreLive;
        public event EventHandler? OnCommandersUpdated;

        public event EventHandler<StatusFileEvent>? StatusUpdated
        {
            add { journalEventParser.StatusUpdated += value; }
            remove { journalEventParser.StatusUpdated -= value; }
        }
        #endregion

        private bool isLive;
        public bool IsLive 
        { 
            get => isLive; 
            private set 
            { 
                isLive = value; 
                OnParserStoreLive?.Invoke(this, value); 
            } 
        }
        public bool Odyssey => !journalEventParser.Legacy;
        public List<JournalCommander> JournalCommanders => _journalCommanders;
        public NavigationRoute? GetNavRoute() => journalEventParser.ReadNavRouteJson(settingsStore.SelectedCommanderID);

        public void RegisterParser(IProcessJournalLogs journalLogParser)
        {
            if (journalLogParserList.Contains(journalLogParser))
                return;

            journalLogParserList.Add(journalLogParser);
        }

        public void UnregisterParser(IProcessJournalLogs journalLogParser)
        {
            journalLogParserList.Remove(journalLogParser);
        }

        public void ReadNewCommander(int commanderID)
        {
            IsLive = false;

            foreach (var parser in journalLogParserList)
            {
                parser.ClearData();
            }
            settingsStore.SelectedCommanderID = commanderID;

            var commander = odToolsDatabase.GetCommander(commanderID)
                        ?? new(0, "", "", "", false);

            if (!journalEventParser.StartWatching(commander, null))
            {
                IsLive = true;
            }
        }

        public void ReadNewDirectory(string path)
        {
            IsLive = false;

            foreach (var parser in journalLogParserList)
            {
                parser.ClearData();
            }

            settingsStore.SelectedCommanderID = -1;

            var commander = new JournalCommander(-1, "", path, null, false);

            if (!journalEventParser.StartWatching(commander, null))
            {
                IsLive = true;
            }
        }

        private async Task OnJournalLiveStatusChange(bool e)
        {
            if (e == false)
                return;

            if (settingsStore.SelectedCommanderID < 0)
            {
                settingsStore.SelectedCommanderID = journalEventParser.JournalCMDR?.Id ?? 0;
            }

            settingsStore.SaveSettings();

            var commander = await UpdateCommanders();

            if (commander == null)
            {
                OnJournalStoreStatusChange?.Invoke(this, $"No Commanders Found");
                IsLive = true;
                return;
            }

            OnJournalStoreStatusChange?.Invoke(this, $"Processing History for CMDR {commander.Name}");

            var history = journalLogParserList.Select(x => x.GetEventsToParse(settingsStore.JournalAgeDateTime))
                .Where(x => x.Types.Count > 0).ToList();

            foreach (var logParser in journalLogParserList)
            {
                logParser.RunBeforeParsingLogs(settingsStore.SelectedCommanderID);
            }

            await journalEventParser.StreamJournalHistoryOfTypeAsync(settingsStore.SelectedCommanderID, history);

            OnJournalStoreStatusChange?.Invoke(this, $"Completed");

            IsLive = true;
        }

        public async Task ResetDataBase(OdExplorerDatabaseProvider provider)
        {
            IsLive = false;
            journalEventParser.StopWatcher();
            _journalCommanders.Clear();
            OnCommandersUpdated?.Invoke(this, EventArgs.Empty);
            await Task.Factory.StartNew(provider.ResetDataBaseAsync);
        }

        public async Task<JournalCommander?> UpdateCommanders()
        {
            var commanders = await odToolsDatabase.GetAllJournalCommanders();
            _journalCommanders.Clear();
            _journalCommanders.AddRange(commanders);

            JournalCommander? ret = _journalCommanders.FirstOrDefault(x => x.Id == settingsStore.SelectedCommanderID);
            //If we haven't found any commanders yet, set the first one
            if (settingsStore.SelectedCommanderID <= 0 && _journalCommanders.Count != 0)
            {
                ret = _journalCommanders.FirstOrDefault();
                settingsStore.SelectedCommanderID = ret?.Id ?? 0;
            }
            OnCommandersUpdated?.Invoke(this, EventArgs.Empty);

            return ret;
        }

        private void OnJournalEventReceived(object? sender, JournalEntry e)
        {
            if (e.CommanderID != settingsStore.SelectedCommanderID)
            {
                return;
            }

            foreach (var parser in journalLogParserList)
            {
                parser.ParseJournalEvent(e);
            }
        }

        private void OnReadingNewJournalFile(object? sender, string e)
        {
            OnJournalStoreStatusChange?.Invoke(null, e);
        }
    }
}
