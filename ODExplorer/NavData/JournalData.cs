using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.AppSettings;
using ODExplorer.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ODExplorer.NavData
{
    public class JournalData : PropertyChangeNotify
    {
        private readonly JournalWatcher _watcher;

        private NavigationData _navData;

        private DateTime NavJsonLastWriteTime = new(DateTime.MinValue.Ticks);

        private Settings _appSettings;
        public JournalData(Settings settings)
        {
            _appSettings = settings;
            //create watcher
            _watcher = new JournalWatcher(_appSettings.Value.JournalPath);

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

            _watcher.GetEvent<SellOrganicDataEvent>().AddHandler(OnSellOrganic);

            _watcher.GetEvent<ApproachBodyEvent>()?.AddHandler(OnApproachBody);
        }

        private void OnApproachBody(object sender, ApproachBodyEvent.ApproachBodyEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnApproachBody(e);
        }

        public bool WatcherLive => _watcher.IsLive;

        public TJournalEvent GetEvent<TJournalEvent>() where TJournalEvent : JournalEvent
        {
            return _watcher.GetEvent<TJournalEvent>();
        }

        private void CodexEntry(object sender, CodexEntryEvent.CodexEntryEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            _navData.OnCodexEntry(e);
        }

        private async void SupercruiseExit(object sender, SupercruiseExitEvent.SupercruiseExitEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                //If we are just starting up and reading journals, give the location event time to work
                //await Task.Delay(5000);
                return;
            }

            await _navData.OnSupercruiseExit(e);
        }

        private void SupercruiseEntry(object sender, SupercruiseEntryEvent.SupercruiseEntryEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                //If we are just starting up and reading journals, give the location event time to work
                //await Task.Delay(5000);
                return;
            }

            _navData.OnSupercruiseEntry();
        }

        public void StartWatcher(NavigationData navData)
        {
            _navData = navData;
            //Start watcher
            _ = _watcher.StartWatching().ConfigureAwait(false);
//#if DEBUG
//            _ =ReadNavRouteJson();
//#endif
        }

        private void FileHeader(object sender, FileheaderEvent.FileheaderEventArgs e)
        {
            NavigationData.Odyssey = e.Odyssey;
        }

        private async void Location(object sender, LocationEvent.LocationEventArgs e)
        {
            //if (_watcher.IsLive == false)
            //{
            //    return;
            //}

            SystemInfo sys = new(e);

            await _navData.SetCurrentSystem(sys);
            await _navData.SetCurrentBody(e);
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

        private async void NavRoute(object sender, NavRouteEvent.NavRouteEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            await ReadNavRouteJson();
        }

        /// <summary>
        /// Reads the data from NavRoute.json in the journal log directory
        /// </summary>
        private async ValueTask ReadNavRouteJson()
        {
            string path = Path.Combine(_appSettings.Value.JournalPath, "NavRoute.json");

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

            await _navData.PopulateRoute(route);
        }

        private async void StartJump(object sender, StartJumpEvent.StartJumpEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;

            }

            if (e.JumpType == JumpType.Hyperspace)
            {
                await _navData.StartJump(e);
            }
        }

        private async void FSDJump(object sender, FSDJumpEvent.FSDJumpEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                //If we are just starting up and reading journals, give the location event time to work
                //await Task.Delay(5000);
                return;
            }

            await _navData.OnFSDJump(e);
        }

        private async void FSDTarget(object sender, FSDTargetEvent.FSDTargetEventArgs e)
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

            await _navData.OnFsdTarget(sys);
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

        private async void ScanOrganic(object sender, ScanOrganicEvent.ScanOrganicEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                return;
            }

            await _navData.ScanOrganic(e);
        }

        private void OnSellOrganic(object sender, SellOrganicDataEvent.SellOrganicDataEventArgs e)
        {
            if (_watcher.IsLive == false)
            {
                //return;
            }

            _navData.SellOrganic(e);
        }
    }
}
