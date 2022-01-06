using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.Utils;
using System;
using System.IO;

namespace ODExplorer.NavData
{
    public class JournalData : PropertyChangeNotify
    {
        private readonly JournalWatcher _watcher;

        private readonly string journalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Saved Games",
            "Frontier Developments",
            "Elite Dangerous");

        private NavigationData _navData;

        private DateTime NavJsonLastWriteTime = new(DateTime.MinValue.Ticks);

        public JournalData()
        {
            //create watcher
            _watcher = new JournalWatcher(journalPath);

            //Subscribe to events
            _watcher.GetEvent<FSDJumpEvent>()?.AddHandler(FSDJump);//

            _watcher.GetEvent<StartJumpEvent>()?.AddHandler(StartJump);//

            _watcher.GetEvent<ScanEvent>()?.AddHandler(Scan);//

            _watcher.GetEvent<LocationEvent>()?.AddHandler(Location);//

            _watcher.GetEvent<FSSDiscoveryScanEvent>()?.AddHandler(DiscoveryScan);//

            _watcher.GetEvent<FSSAllBodiesFoundEvent>()?.AddHandler(AllBodiesFound);//

            _watcher.GetEvent<NavRouteEvent>()?.AddHandler(NavRoute);//

            _watcher.GetEvent<FSDTargetEvent>()?.AddHandler(FSDTarget);//

            _watcher.GetEvent<FileheaderEvent>()?.AddHandler(FileHeader);//

            _watcher.GetEvent<FSSBodySignalsEvent>()?.AddHandler(FSSBodySignals);//

            _watcher.GetEvent<SAASignalsFoundEvent>()?.AddHandler(SAASignalsFound);//

            _watcher.GetEvent<SAAScanCompleteEvent>()?.AddHandler(DSScanComplete);//

            _watcher.GetEvent<SellExplorationDataEvent>()?.AddHandler(SellExplorationData);//

            _watcher.GetEvent<MultiSellExplorationDataEvent>()?.AddHandler(SellExplorationData);//

            _watcher.GetEvent<ScanOrganicEvent>()?.AddHandler(ScanOrganic);//

            _watcher.GetEvent<SupercruiseEntryEvent>()?.AddHandler(SupercruiseEntry);

            _watcher.GetEvent<SupercruiseExitEvent>()?.AddHandler(SupercruiseExit);

            _watcher.GetEvent<CodexEntryEvent>()?.AddHandler(CodexEntry);
        }

        private void CodexEntry(object sender, CodexEntryEvent.CodexEntryEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnCodexEntry(e);
        }

        private void SupercruiseExit(object sender, SupercruiseExitEvent.SupercruiseExitEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnSupercruiseExit(e);
        }

        private void SupercruiseEntry(object sender, SupercruiseEntryEvent.SupercruiseEntryEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnSupercruiseEntry();
        }

        public void StartWatcher(NavigationData navData)
        {
            _navData = navData;
            //Start watcher
            _ = _watcher.StartWatching().ConfigureAwait(false);
#if DEBUG
            //ReadNavRouteJson();
#endif
        }

        private void FileHeader(object sender, FileheaderEvent.FileheaderEventArgs e)
        {
            NavigationData.Odyssey = e.Odyssey;
        }

        private void Location(object sender, LocationEvent.LocationEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            SystemInfo sys = new(e);

            _navData.SetCurrentSystem(sys);
            _navData.SetCurrentBody(e);
        }

        private void Scan(object sender, ScanEvent.ScanEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnScan(e);
        }

        private void FSSBodySignals(object sender, FSSBodySignalsEvent.FSSBodySignalsEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _ = _navData.FSSBodySignals(e);
        }

        private void SAASignalsFound(object sender, SAASignalsFoundEvent.SAASignalsFoundEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _ = _navData.SAASignalsFound(e);
        }

        private void DiscoveryScan(object sender, FSSDiscoveryScanEvent.FSSDiscoveryScanEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnDiscoveryScan(e);
        }

        private void DSScanComplete(object sender, SAAScanCompleteEvent.SAAScanCompleteEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _ = _navData.OnDSScanComplete(e);
        }

        private void AllBodiesFound(object sender, FSSAllBodiesFoundEvent.FSSAllBodiesFoundEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            if (_navData.CurrentSystem.Count < 0)
            {
                return;
            }

            _navData.AllBodiesFound(e);
        }

        private void NavRoute(object sender, NavRouteEvent.NavRouteEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            ReadNavRouteJson();
        }

        /// <summary>
        /// Reads the data from NavRoute.json in the journal log directory
        /// </summary>
        private void ReadNavRouteJson()
        {
            string path = Path.Combine(journalPath, "NavRoute.json");

            if (!File.Exists(path))
            {
                return;
            }

            //Check if the json has been updated since we last read it.
            DateTime lastWriteTime = File.GetLastWriteTime(path);

            if (lastWriteTime <= NavJsonLastWriteTime)
            {
                return;
            }

            NavJsonLastWriteTime = lastWriteTime;

            string json = File.ReadAllText(path);

            NavigationRoute route = NavigationRoute.FromJson(json);

            _navData.PopulateRoute(route);
        }

        private void StartJump(object sender, StartJumpEvent.StartJumpEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;

            }

            if (e.JumpType == JumpType.Hyperspace)
            {
                _navData.StartJump(e);
            }
        }

        private void FSDJump(object sender, FSDJumpEvent.FSDJumpEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnFSDJump(e);
        }

        private void FSDTarget(object sender, FSDTargetEvent.FSDTargetEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            SystemInfo sys = new()
            {
                SystemName = e.Name.ToUpperInvariant(),
                SystemAddress = e.SystemAddress,
                StarClass = e.StarClass,
            };

            _navData.OnFsdTarget(sys);
        }

        private void SellExplorationData(object sender, MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.SellExplorationData(e);
        }

        private void SellExplorationData(object sender, SellExplorationDataEvent.SellExplorationDataEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.SellExplorationData(e);
        }

        private void ScanOrganic(object sender, ScanOrganicEvent.ScanOrganicEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.ScanOrganic(e);
        }
    }
}
