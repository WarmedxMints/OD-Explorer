using EliteJournalReader;
using EliteJournalReader.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ODExplorer.AppSettings;
using ODExplorer.EDSM;
using ODExplorer.GeologicalData;
using ODExplorer.OrganicData;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Drawing.Design;

namespace ODExplorer.NavData
{
    public class NavigationData : PropertyChangeNotify
    {
        #region Events
        public delegate void OnCurrentSystemChange(SystemInfo systemInfo);

        public event OnCurrentSystemChange OnCurrentSystemChanged;
        #endregion
        #region Properties
        public Settings AppSettings { get; set; }

        //Estimated Scan Values
        private EstimatedScanValue _scanValue = new();
        public EstimatedScanValue ScanValue { get => _scanValue; set { _scanValue = value; OnPropertyChanged(); } }

        //Scanned Bio Data
        private ScannedBioData _scannedBioData = new();
        public ScannedBioData ScannedBio { get => _scannedBioData; set { _scannedBioData = value; OnPropertyChanged(); } }

        //Scneed Geo Data
        private ScannedGeoData _scannedGeo = new();
        public ScannedGeoData ScannedGeo { get => _scannedGeo; set { _scannedGeo = value; OnPropertyChanged(); } }

        //Collection for the current system
        private ObservableCollection<SystemInfo> _currentSystem = new();
        public ObservableCollection<SystemInfo> CurrentSystem { get => _currentSystem; set { _currentSystem = value; OnPropertyChanged(); } }

        //Collection for the systems in route between the current system and the destination
        private ObservableCollection<SystemInfo> _systemsInRoute = new();
        public ObservableCollection<SystemInfo> SystemsInRoute { get => _systemsInRoute; set { _systemsInRoute = value; OnPropertyChanged(); } }

        //This collection is a backup just in case the user targets another system not in the route and then
        //re-targets the next system in the calulated route
        private ObservableCollection<SystemInfo> _backupRoute = new();
        //This is a reference to the system the user started a hyperspace jump to
        private SystemInfo _lastJumpSystem;
        public SystemInfo LastJumpSystem { get => _lastJumpSystem; set { _lastJumpSystem = value; OnPropertyChanged("HyperSpaceText"); } }
        public string HyperSpaceText => InHyperSpace ? LastJumpSystem == null ? "HYPERSPACE" : $"JUMPING TO {LastJumpSystem.SystemName}" : "WAITING FOR DATA";
        //The body we are current on/in orbit of if any
        private SystemBody currentBody;
        public SystemBody CurrentBody { get => currentBody; set { currentBody = value; OnPropertyChanged(); } }
        //Exploration Values are tweaked in Odyssey       
        public static bool Odyssey { get; set; }
        //Bool set to true when the user is jumping
        private bool _inHyperSpace;
        public bool InHyperSpace { get => _inHyperSpace; set { _inHyperSpace = value; OnPropertyChanged(); OnPropertyChanged("HyperSpaceText"); CurrentBody = null; } }

        //This is set to true when we are populating a route for NavRoute.json as an fsd targeted event will also be called
        //and we would want to ignore it in that case.
        private bool _populatingRoute;
        public bool PopulatingRoute { get => _populatingRoute; set { _populatingRoute = value; OnPropertyChanged(); } }
        #endregion

        #region Scanning Menthods
        //This method is called when the user honks a system
        public void OnDiscoveryScan(FSSDiscoveryScanEvent.FSSDiscoveryScanEventArgs e)
        {
            if (CurrentSystem == null || !CurrentSystem.Any())
            {
                return;
            }
            //Check if the current system matches the honked system
            if (e.SystemAddress != CurrentSystem[0].SystemAddress)
            {
                return;
            }
            //Update the gui with the number of bodies the disvocery scanner has found
            //The user can than see if EDSM is unaware of some of the bodies
            CurrentSystem[0].DiscoveredBodiesCount = e.BodyCount;
            //If the honk completes a system scan, update the progress bar on the gui
            CurrentSystem[0].AllBodiesFound = e.Progress >= 1;
        }

        //Called when the AllBodiesFound even is fired
        public void AllBodiesFound(FSSAllBodiesFoundEvent.FSSAllBodiesFoundEventArgs e)
        {
            if (CurrentSystem == null || CurrentSystem.Count <= 0)
            {
                return;
            }
            //Check if the current system matches
            if (e.SystemAddress != CurrentSystem[0].SystemAddress)
            {
                return;
            }

            CurrentSystem[0].AllBodiesFound = true;
        }

        //This is called when a scan is performed on any body.  That could be an auto scan 
        //or one using a FSScanner.
        public void OnScan(ScanEvent.ScanEventArgs e)
        {
            if (!CurrentSystem.Any())
            {
                return;
            }
            //Check the body is part of the system we are currently in
            if (!string.Equals(e.StarSystem, CurrentSystem[0].SystemName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            //Create a new body.  No need to check if it alread exists as that is done in the AddBody method
            SystemBody body = new(e, Odyssey);
            //Add the body to the current system
            SystemBody bodyToAdd = AddBodyToCurrentSystem(true, false, body);


            if (bodyToAdd is not null)
            {
                bodyToAdd.UpdateStatus();
                ScanValue.UpdateMainStarValue(CurrentSystem[0]);
            }
        }

        public async Task OnDSScanComplete(SAAScanCompleteEvent.SAAScanCompleteEventArgs e)
        {
            if (!CurrentSystem.Any())
            {
                return;
            }

            int i = 0;
        TryAgain:

            if (i > 20)
            {
                return;
            }

            //Find the body we have just mapped
            SystemBody body = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyName.ToLowerInvariant() == e.BodyName.ToLowerInvariant());

            if (body == default)
            {
                i++;
                await Task.Delay(100);
                goto TryAgain;
            }

            if (body is not null)
            {
                body.MappedByUser = true;
                body.EffeicentMapped = e.ProbesUsed <= e.EfficiencyTarget;
                //Calc our final mapped value
                body.MappedValue = MathFunctions.GetBodyValue(body, Odyssey, true, body.EffeicentMapped);
                body.UpdateStatus();
                ScanValue.UpdatesEstimatedScanValue();
            }
        }

        //This is called when a scan with the FSScanner detects biological or geological signals
        public async Task FSSBodySignals(FSSBodySignalsEvent.FSSBodySignalsEventArgs e)
        {
            //Failsafe which should never be needed but just in case check the event contains data 
            //and we have a current system.
            if (e.Signals == null || e.Signals.Length < 1 || !CurrentSystem.Any())
            {
                return;
            }

            //The body should be there but the events are trigged at the same time so we must wait a little for it to be created.
            //TODO : Fix this dirty workaround
            //Try for 2 seconds and then give up
            int i = 0;
        TryAgain:

            if (i > 20)
            {
                return;
            }

            await Task.Delay(100);
            //Find the body we have just scanned in our collection on known bodies
            SystemBody body = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyName.ToUpperInvariant() == e.BodyName.ToUpperInvariant());

            if (body == default)
            {
                i++;
                goto TryAgain;
            }

            int geo = 0;
            int bio = 0;

            foreach (FSSSignal signal in e.Signals)
            {
                switch (signal.Type)
                {
                    case "$SAA_SignalType_Biological;":
                        bio = signal.Count;
                        break;
                    case "$SAA_SignalType_Geological;":
                        geo = signal.Count;
                        break;
                    default:
                        break;
                }
            }
            //Add the detected signal count to our body
            body.BiologicalSignals = bio;
            body.GeologicalSignals = geo;
            body.PopulateNotables();
            body.UpdateStatus();
        }

        public async Task SAASignalsFound(SAASignalsFoundEvent.SAASignalsFoundEventArgs e)
        {
            //Failsafe which should never be needed but just in case check the event contains data 
            //and we have a current system.
            if (e.Signals == null || e.Signals.Length < 1 || !CurrentSystem.Any())
            {
                return;
            }

            int i = 0;
        TryAgain:

            if (i > 20)
            {
                return;
            }

            await Task.Delay(100);
            //Find the body we have just scanned in our collection on known bodies
            SystemBody body = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyName.ToUpperInvariant() == e.BodyName.ToUpperInvariant());

            if (body == default)
            {
                i++;
                goto TryAgain;
            }

            int geo = 0;
            int bio = 0;

            foreach (SAASignal signal in e.Signals)
            {
                switch (signal.Type_Localised)
                {
                    case "Biological":
                        bio = signal.Count;
                        break;
                    case "Geological":
                        geo = signal.Count;
                        break;
                    default:
                        break;
                }
            }
            //Add the detected signal count to our body
            body.BiologicalSignals = bio;
            body.GeologicalSignals = geo;
            body.UpdateStatus();
        }

        public async ValueTask ScanOrganic(ScanOrganicEvent.ScanOrganicEventArgs e)
        {
            CurrentBody ??= await GetSystemBodyFromEDSM(e.SystemAddress, e.Body);

            ScannedBio.AddData(CurrentBody, e.Species_Localised.ToUpperInvariant(), e.ScanType, e.EliteTimeString);
        }

        internal void OnCodexEntry(CodexEntryEvent.CodexEntryEventArgs e)
        {
            if (CurrentBody is null)
            {
                return;
            }

            //Geo Scan
            if (e.SubCategory.Contains("Geology"))
            {
                ScannedGeo.AddCodexData(e.System.ToUpperInvariant(), CurrentBody.BodyName, e.Name_Localised.ToUpperInvariant(), e.VoucherAmount ?? 0);
                return;


            }

            //Bio Scan
            if (e.SubCategory.Contains("Organic"))
            {
                ScannedBio.AddCodexData(CurrentBody, e.EliteTimeString, e.Name_Localised.ToUpperInvariant());
                return;
            }
        }

        internal void SellOrganic(SellOrganicDataEvent.SellOrganicDataEventArgs e)
        {
            ScannedBio.SellOrganic(e);
        }
        #endregion

        #region FSD Methods
        //Called whenever a system is auto or manually targeted
        public async ValueTask OnFsdTarget(SystemInfo sys)
        {
            //FSD Target event for the next system in route fires while in hyperspace
            //and when a route has been plotted so we ignore this event in those cases
            if ((InHyperSpace && SystemsInRoute.Any()) || _populatingRoute)
            {
                return;
            }

            //Check if we are simply targetting the next sys in the known route
            if (SystemsInRoute.Any() && sys.SystemAddress == SystemsInRoute[0].SystemAddress)
            {
                return;
            }
            //Check if we are targetting the first system in the backupRoute.
            //This is for a senario where the user targeted a different system and
            //then re-targeted the next system in the route
            if (_backupRoute.Any() && sys.SystemAddress == _backupRoute[0].SystemAddress)
            {
                SystemsInRoute.ClearCollection();
                SystemsInRoute.AddRangeToCollection(_backupRoute);
                return;
            }

            //If the system is not in our route, clear the collection
            SystemsInRoute.ClearCollection();
            //Get the scan value of the system from EDSM
            await GetSystemValue(sys);
            sys.SystemPos = await GetSystemPosition(sys.SystemName);
            UpdateRemainingJumpDistace();
            //Add the system to our collection
            SystemsInRoute.AddToCollection(sys);
        }

        //Called when a hyperspace jump is started
        public async ValueTask StartJump(StartJumpEvent.StartJumpEventArgs e)
        {
            InHyperSpace = true;

            SystemInfo sys;

            if (SystemsInRoute.Any() && e.SystemAddress == SystemsInRoute[0].SystemAddress)
            {
                sys = SystemsInRoute[0];
                SystemsInRoute.RemoveFromCollection(sys);
                UpdateBackupRoute();
            }
            else
            {
                sys = new()
                {
                    SystemName = e.StarSystem.ToUpperInvariant(),
                    SystemAddress = e.SystemAddress,
                    StarClass = e.StarClass.ToUpperInvariant()
                };
            }

            await GetEDSMBodyCount(sys);  

            LastJumpSystem = sys;

            CurrentSystem.ClearCollection();

            Settings.SettingsInstance.SaveAll();
        }

        //Called when a hyperspace jump has been completed
        public async ValueTask OnFSDJump(FSDJumpEvent.FSDJumpEventArgs e)
        {
            //Ship is no longer in hyperspace
            InHyperSpace = false;

            //Check if we have jumped to a system in our route
            SystemInfo sys = SystemsInRoute.FirstOrDefault(x => x.SystemAddress == e.SystemAddress);

            if (sys is not null)
            {
                //Just in case the user has jumped to a sys part way through the route
                //we will remove the skipped systems as well
                //Probably isn't required as a new route would be calculated
                //Although in my experience this does not always ping a NavRoute event
                SystemsInRoute.RemoveAllBeforeItem(sys, true);

                if (sys.SystemPos.IsZero())
                {
                    sys.SystemPos = e.StarPos.Copy();
                }
                //Set the current system to the one we just jumped into
                await SetCurrentSystem(sys);
                UpdateBackupRoute();
                return;
            }

            //We jumped 
            sys = _backupRoute.FirstOrDefault(x => x.SystemAddress == e.SystemAddress);

            if (sys is not null)
            {
                int index = _backupRoute.IndexOf(sys);
                //Just in case the user has jumped to a sys part way through the route
                //we will remove the skipped systems as well
                //Probably isn't required as a new route would be calculated
                //Although in my experience this does not always ping a NavRoute event
                for (int i = index; i > 0; i--)
                {
                    _backupRoute.RemoveAt(i);
                }

                if (sys.SystemPos.IsZero())
                {
                    sys.SystemPos = e.StarPos.Copy();
                }
                //Set the current system to the one we just jumped into
                await SetCurrentSystem(sys);

                //If we have any systems left in our route, add them to our current route
                if (_backupRoute.Count > 0)
                {
                    SystemsInRoute.ClearCollection();
                    SystemsInRoute.AddRangeToCollection(_backupRoute);
                }
                return;
            }
            //If we have got this far, we have just jumped into a system not on the known route
            //Check if we know about the system and set it as aour current system
            if (LastJumpSystem != null)
            {
                sys = LastJumpSystem;

                sys.SystemPos = e.StarPos.Copy();
                LastJumpSystem = null;

                await GetSystemValue(sys);

                await SetCurrentSystem(sys, false);

                return;
            }
            //We must have missed the start the jump so create a new systeminfo and set it as out current system
            sys = new SystemInfo
            {
                SystemName = e.StarSystem.ToUpperInvariant(),
                SystemAddress = e.SystemAddress,
                SystemPos = e.StarPos.Copy()
            };

            await GetSystemValue(sys);

            await SetCurrentSystem(sys);
        }
        #endregion

        #region System Population
        //Called when the NavRoute event is fired and the NavRoute.json file has been read
        public async ValueTask PopulateRoute(NavigationRoute route)
        {
            //Null checks
            if (route is null || route.Route is null)
            {
                return;
            }

            int count = route.Route.Count;
            //Check we have something in our route
            if (count <= 0)
            {
                return;
            }
            //We have data and are populating a route
            PopulatingRoute = true;

            //Clear our collections
            SystemsInRoute.ClearCollection();

            List<Route> systems = route.Route;

            SystemInfo currentSys = new(systems[0]);
            SystemPosition lastPos = currentSys.SystemPos;

            //populate the current system
            await SetCurrentSystem(currentSys);
            double totaldistance = 0;

            //populate the systems in route and create the backup
            for (int i = 1; i < count; i++)
            {
                SystemInfo sys = new(systems[i]);
                sys.JumpDistanceToSystem = SystemPosition.Distance(lastPos, sys.SystemPos);
                totaldistance += sys.JumpDistanceToSystem;
                sys.JumpDistanceRemaining = (int)Math.Round(totaldistance);
                await GetSystemValue(sys);
                SystemsInRoute.AddToCollection(sys);
                lastPos = sys.SystemPos;
            }
            UpdateBackupRoute();
            //UpdateRemainingJumpDistace();

            PopulatingRoute = false;
        }
        //Method to set the current system
        public async ValueTask SetCurrentSystem(SystemInfo sys, bool getCount = true)
        {
            //Check we aren't already in the system
            SystemInfo currentSystem = CurrentSystem.FirstOrDefault(x => x.SystemAddress == sys.SystemAddress);
            //If we are in the system, return
            if (currentSystem is not null)
            {
                UpdateRemainingJumpDistace();
                return;
            }
            //Check we don't have unsold Navigation History
            SystemInfo knownSystem = ScanValue.ScannedSystems.FirstOrDefault(x => x.SystemAddress == sys.SystemAddress);
            //If we do
            if (knownSystem is not null)
            {
                //Update EDSM Data
                knownSystem.PolledEDSMValue = false;
                if (getCount)
                {
                    await GetEDSMBodyCount(knownSystem);
                }
                await GetSystemValue(knownSystem);
                //Make it the current system
                CurrentSystem.ClearCollection();
                CurrentSystem.AddToCollection(knownSystem);
                UpdateRemainingJumpDistace();
                //Add edsm bodies although they are likely already in there.
                //We will add them just for the rare case something has been discovered since.
                foreach (SystemBody body in sys.Bodies)
                {
                    //Check if we already have the body in the collection
                    SystemBody bodyknown = knownSystem.Bodies.FirstOrDefault(x => x.BodyName == body.BodyName);

                    if (bodyknown == default)
                    {
                        _ = AddBodyToCurrentSystem(false, true, body);
                    }
                }
                OnCurrentSystemChanged?.Invoke(knownSystem);
                return;
            }

            //We aren't aware of the current system so clear the collection and add it
            CurrentSystem.ClearCollection();
            CurrentSystem.AddToCollection(sys);
            UpdateRemainingJumpDistace();
            if (getCount)
            {
                await GetEDSMBodyCount(sys);
            }
            //If our system information is missing the main star details, get it from EDSM
            if (string.IsNullOrEmpty(sys.StarClass))
            {
                sys.StarClass = await GetSystemStarClass(sys.SystemName);
            }
            //If we don't have a scan value for the system, get it from EDSM
            if (!sys.PolledEDSMValue)
            {
                await GetSystemValue(sys);
            }

            OnCurrentSystemChanged?.Invoke(sys);
        }
        //Update remaining jump distance
        private void UpdateRemainingJumpDistace()
        {
            if (!SystemsInRoute.Any() && !CurrentSystem.Any())
            {
                return;
            }

            double totaldistance = 0;
            bool isSol = false;
            SystemPosition lastPos = CurrentSystem[0].SystemPos;
            isSol = CurrentSystem[0].SystemName.Equals("SOL", StringComparison.OrdinalIgnoreCase);

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (SystemInfo sys in SystemsInRoute)
                {
                    if (lastPos.IsZero() && !isSol)
                    {
                        sys.JumpDistanceToSystem = 0;
                        lastPos = sys.SystemPos;
                        isSol = sys.SystemName.Equals("SOL", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }
                    sys.JumpDistanceToSystem = SystemPosition.Distance(lastPos, sys.SystemPos);
                    totaldistance += sys.JumpDistanceToSystem;
                    sys.JumpDistanceRemaining = (int)Math.Round(totaldistance);
                    lastPos = sys.SystemPos;
                    isSol = sys.SystemName.Equals("SOL", StringComparison.OrdinalIgnoreCase);
                }
            });
        }
        //Adds a body to the current system
        public SystemBody AddBodyToCurrentSystem(bool fromDetailedScan, bool fromEDSM, SystemBody bodyToAdd)
        {
            if (bodyToAdd.IsNonBody && AppSettings.Value.IgnoreNonBodies)
            {
                //Do nothing as we are currently ignoring non bodies
                return null;
            }

            //Check we have a current system
            if (!CurrentSystem.Any())
            {
                return null;
            }
            //Check if we already have the body in the collection
            SystemBody bodyknown = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyName.ToUpperInvariant() == bodyToAdd.BodyName.ToUpperInvariant());

            if (bodyknown == default)
            {
                CurrentSystem[0].Bodies.AddToCollection(bodyToAdd);
                //Abirtary value just to call the OnpropertyChaned event for this proprety 
                CurrentSystem[0].PercentageScanned = 0;
                return bodyToAdd;
            }

            if (fromDetailedScan)
            {
                bodyknown.UpdateFromDetailedScan(bodyToAdd, Odyssey);
            }

            if (fromEDSM)
            {
                bodyknown.UpdateFromEDSM(bodyToAdd);
            }

            return bodyToAdd;
        }

        private void UpdateBackupRoute()
        {
            _backupRoute.Clear();
            _backupRoute = new(SystemsInRoute);
        }
        #endregion

        #region Data Sale Methods
        internal void SellExplorationData(MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs e)
        {
            ScanValue.SellExplorationData(e);
        }

        internal void SellExplorationData(SellExplorationDataEvent.SellExplorationDataEventArgs e)
        {
            ScanValue.SellExplorationData(e);
        }
        #endregion

        #region Current Body Methods
        internal async ValueTask OnSupercruiseExit(SupercruiseExitEvent.SupercruiseExitEventArgs e)
        {
            if (!CurrentSystem.Any() || CurrentSystem[0].SystemAddress != e.SystemAddress)
            {
                //Shouldn't be required but just in case a jump to SC or HS was missed for some reason
                CurrentBody = null;
                return;
            }

            SystemBody body = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyID == e.BodyID);

            body ??= await GetSystemBodyFromEDSM(e.SystemAddress, e.BodyID);

            CurrentBody = body;
        }

        internal void OnSupercruiseEntry() => CurrentBody = null;

        internal async ValueTask SetCurrentBody(LocationEvent.LocationEventArgs e)
        {
            if (e.BodyType != BodyType.Planet)
            {
                CurrentBody = null;
                return;
            }

            SystemBody body = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyID == e.BodyID);

            body ??= await GetSystemBodyFromEDSM(e.SystemAddress, e.BodyID);

            CurrentBody = body;
        }
        #endregion

        #region Body Methods
        //Refresh the status enum on Body objects.
        //This is used to referesh the display on the current system bodies datagrid
        public void RefreshBodiesStatus()
        {
            if (CurrentSystem.Any() == false)
            {
                return;
            }

            ObservableCollection<SystemBody> boides = CurrentSystem[0].Bodies;

            foreach (SystemBody body in boides)
            {
                body.UpdateUI();
            }
        }
        #endregion

        #region EDSM Methods
        private static async ValueTask<SystemPosition> GetSystemPosition(string systemName)
        {
            SystemPosition ret = new();

            string baseUrl = "https://www.edsm.net";
            string path = $"api-v1/system?systemName={HttpUtility.UrlEncode(systemName)}&showCoordinates=1";

            string json = await GetEDSMJson(baseUrl, path);

            if (string.IsNullOrEmpty(json))
            {
                return ret;
            }
            try
            {

                JObject msg = JObject.Parse(json);

                JObject coords = msg["coords"] as JObject;

                ret.X = coords["x"].ToObject<double>();
                ret.Y = coords["y"].ToObject<double>();
                ret.Z = coords["z"].ToObject<double>();


                return ret;
            }
            catch
            {
                return ret;
            }
        }
        /// <summary>
        /// Contacts EDSM and gets the class of the primary star
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        private static async ValueTask<string> GetSystemStarClass(string systemName)
        {
            string baseUrl = "https://www.edsm.net";

            string path = $"api-v1/system?systemName={HttpUtility.UrlEncode(systemName)}&showPrimaryStar=1";

            string json = await GetEDSMJson(baseUrl, path);

            if (string.IsNullOrEmpty(json))
            {
                return string.Empty;
            }
            try
            {
                JObject msg = JObject.Parse(json);

                JObject starinfo = msg["primaryStar"] as JObject;

                string starClass = (string)starinfo["type"];

                if(string.IsNullOrEmpty(starClass))
                {
                    return "?";
                }

                string[] sClass = starClass.Split(' ');

                return sClass[0].ToUpperInvariant();
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Gets the known bodycount of a system from EDSM
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        private static async ValueTask GetEDSMBodyCount(SystemInfo system)
        {
            string baseUrl = "https://www.edsm.net";

            string path = $"api-system-v1/bodies?systemName={system.SystemName}";

            string json = await GetEDSMJson(baseUrl, path);

            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            try
            {
                if (json.Length < 5)
                {
                    return;
                }

                JObject msg = JObject.Parse(json);

                system.KnownBodies = (msg["bodies"] as JArray).Count;
            }
            catch (Exception)
            {

            }
        }

        private static async ValueTask<SystemBody> GetSystemBodyFromEDSM(long systemId, long bodyID)
        {
            SystemBody ret = new();

            string baseUrl = "https://www.edsm.net";

            string path = $"api-system-v1/bodies?systemId64={systemId}";

            string json = await GetEDSMJson(baseUrl, path);

            if (string.IsNullOrEmpty(json))
            {
                return ret;
            }

            try
            {
                if (json.Length < 5)
                {
                    return ret;
                }

                JObject msg = JObject.Parse(json);

                ret.SystemAddress = msg["id64"].ToObject<long>();
                ret.SystemName = msg["name"].ToObject<string>().ToUpperInvariant();

                JArray Bodies = msg["bodies"] as JArray;

                foreach (JObject body in Bodies.Cast<JObject>())
                {
                    if ((int)body["bodyId"] == bodyID)
                    {
                        ret.BodyID = body["bodyId"].ToObject<int>();
                        ret.BodyName = body["name"]?.ToObject<string>().ToUpperInvariant();
                        ret.SetBodyNameLocal();
                        ret.PlanetClass = EnumHelpers.ToEnum(body["subType"]?.ToObject<string>(), PlanetClass.Unknown);
                        ret.AtmosphereDescrtiption = body["atmosphereType"]?.ToObject<string>();
                        ret.Volcanism = body["volcanismType"]?.ToObject<string>();
                        ret.SurfacePressure = body["surfacePressure"] == null ? 0 : body["surfacePressure"].ToObject<double>();
                        ret.SurfaceGravity = body["gravity"] == null ? 0 : body["gravity"].ToObject<double>();
                        ret.SurfaceTemp = body["surfaceTemperature"] == null ? 0 : body["surfaceTemperature"].ToObject<int>();

                        return ret;
                    }
                }
            }
            catch (Exception)
            {
                return ret;
            }

            return ret;
        }
        /// <summary>
        /// Gets the estimated value of a system from EDSM
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static async ValueTask GetSystemValue(SystemInfo ret)
        {
            //var ret = new SystemInfo(system);
            //We already have a value
            if (ret.PolledEDSMValue)
            {
                return;
            }
            //var path = $"https://www.edsm.net/api-system-v1/estimated-value?systemName={ret.SystemName}";

            string baseUrl = "https://www.edsm.net";

            string path = $"api-system-v1/estimated-value?systemId64={ret.SystemAddress}";

            string json = await GetEDSMJson(baseUrl, path);

            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            EDSMSystem sys;

            try
            {
                sys = JsonConvert.DeserializeObject<EDSMSystem>(json);
            }
            catch (Exception)
            {
                return;
            }

            if (sys != null)
            {
                if (json.Length <= 5)
                {
                    return;
                }
                ret.SysValue = sys.EstimatedValueMapped;
                ret.EDSMUrl = sys.Url;
                ret.PolledEDSMValue = true;
                foreach (ValuableBody body in sys.ValuableBodies)
                {
                    bool systemKnown = ret.Bodies.FirstOrDefault(x => x.BodyID == body.BodyId) != default;

                    if (systemKnown)
                    {
                        continue;
                    }

                    SystemBody planet = new()
                    {
                        BodyID = body.BodyId,
                        BodyName = body.BodyName,
                        BodyNameLocal = body.BodyName.ToUpperInvariant(),
                        DistanceFromArrivalLs = (int)body.Distance,
                        MappedValue = (int)body.ValueMax,
                        WasDiscovered = true,
                        PlanetClass = PlanetClass.EdsmValuableBody
                    };

                    if (planet.BodyNameLocal.StartsWith(ret.SystemName, StringComparison.OrdinalIgnoreCase))
                    {
                        planet.BodyNameLocal = planet.BodyNameLocal.Remove(0, ret.SystemName.Length + 1).ToUpperInvariant();
                    }
                    ret.AddBody(planet, false, true, Odyssey);
                }

                ret.IsKnownToEDSM = true;
            }
        }
        /// <summary>
        /// gets a json result from EDSM
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static async ValueTask<string> GetEDSMJson(string baseUrl, string url)
        {
            try
            {
                using var client = new HttpClient();

                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsStringAsync();

                return resp;

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
                //request.Method = "GET";
                //request.ContentType = "application/json; charset=utf-8";
                //request.Headers.Add("Accept-Encoding", "gzip,deflate");
                //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                //request.Timeout = 5000;

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Stream dataStream = response.GetResponseStream();
                //StreamReader reader = new(dataStream);
                //string json = reader.ReadToEnd();
                //reader.Close();
                //dataStream.Close();
                //return json;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
