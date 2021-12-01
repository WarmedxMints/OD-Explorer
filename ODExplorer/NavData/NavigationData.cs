using EliteJournalReader;
using EliteJournalReader.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ODExplorer.AppSettings;
using ODExplorer.EDSM;
using ODExplorer.OrganicData;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace ODExplorer.NavData
{
    public class NavigationData : PropertyChangeNotify
    {
        #region Properties
        private Settings _appSettings;
        public Settings AppSettings { get => _appSettings; set => _appSettings = value; }
        //Estimated Scan Values
        private EstimatedScanValue _scanValue = new();
        public EstimatedScanValue ScanValue
        {
            get => _scanValue;
            set
            {
                _scanValue = value;
                OnPropertyChanged();
            }
        }
        //Scanned Bio Data
        private ScannedBioData _scannedBioData = new();
        public ScannedBioData ScannedBio
        {
            get => _scannedBioData;
            set
            {
                _scannedBioData = value;
                OnPropertyChanged();
            }
        }
        //Collection for the current system
        public ObservableCollection<SystemInfo> CurrentSystem { get; set; } = new();

        //Collection for the systems in route between the current system and the destination
        public ObservableCollection<SystemInfo> SystemsInRoute { get; set; } = new();

        internal void SellExplorationData(MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs e)
        {
            ScanValue.SellExplorationData(e);
        }

        internal void SellExplorationData(SellExplorationDataEvent.SellExplorationDataEventArgs e)
        {
            ScanValue.SellExplorationData(e);
        }

        //This collection is a backup just in case the user targets another system not in the route and then
        //re-targets the next system in the calulated route
        private ObservableCollection<SystemInfo> _backupRoute { get; set; } = new();
        //This is a reference to the system the user started a hyperspace jump to
        public SystemInfo LastJumpSystem { get; set; }

        //Exploration Values are tweaked in Odyssey       
        public bool Odyssey { get; set; }
        //Bool set to true when the user is jumping
        private bool _inHyperSpace { get; set; }

        public bool InHyperSpace
        {
            get => _inHyperSpace;
            set
            {
                _inHyperSpace = value;
                //As we never set the value of the notInhpyerspace bool, set it with a value
                //So the gui can be notified via the OnPropertyChanged call
                OnPropertyChanged();
            }
        }

        //This is set to true when we are populating a route for NavRoute.json as an fsd targeted event will also be called
        //and we would want to ignore it in that case.
        private bool _populatingRoute;

        //Does the use wish to ignore non bodies
        public bool IgnoreNonBodies { get; set; } = true;
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
                ScanValue.UpdateMainStarValue(CurrentSystem[0]);
            }
        }

        public void OnDSScanComplete(SAAScanCompleteEvent.SAAScanCompleteEventArgs e)
        {
            if (!CurrentSystem.Any())
            {
                return;
            }
            //Find the body we have just mapped
            SystemBody body = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyName.ToLowerInvariant() == e.BodyName.ToLowerInvariant());

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
        }

        public void SAASignalsFound(SAASignalsFoundEvent.SAASignalsFoundEventArgs e)
        {
            //Failsafe which should never be needed but just in case check the event contains data 
            //and we have a current system.
            if (e.Signals == null || e.Signals.Length < 1 || !CurrentSystem.Any())
            {
                return;
            }

            //Find the body we have just scanned in our collection on known bodies
            SystemBody body = CurrentSystem[0].Bodies.FirstOrDefault(x => x.BodyName.ToUpperInvariant() == e.BodyName.ToUpperInvariant());

            if (body == default)
            {
                return;
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
        }

        public void ScanOrganic(ScanOrganicEvent.ScanOrganicEventArgs e)
        {
            string systemName = "UNKOWN";
            string bodyName = "-";

            SystemInfo system = CurrentSystem.FirstOrDefault(x => x.SystemAddress == e.SystemAddress);
            SystemBody body = system?.Bodies.FirstOrDefault(x => x.BodyID == e.Body);

            if (system is not null && body is not null)
            {
                systemName = system.SystemName.ToUpperInvariant();
                bodyName = body.BodyNameLocal.ToUpperInvariant();
            }
            else
            {
                string[] names = GeBodyNameFromEDSM(e.SystemAddress, e.Body);
                systemName = names[0];
                bodyName = names[1];
            }

            ScannedBio.AddData(systemName, bodyName, e.Species_Localised.ToUpperInvariant(), e.ScanType);
        }
        #endregion

        #region FSD Methods
        //Called whenever a system is auto or manually targeted
        public void OnFsdTarget(SystemInfo sys)
        {
            //FSD Target event for the next system in route fires while in hyperspace
            //and when a route has been plotted so we ignore this event in those cases
            if (InHyperSpace && SystemsInRoute.Any() || _populatingRoute)
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
                SystemsInRoute = new ObservableCollection<SystemInfo>(_backupRoute);
                return;
            }

            //If the system is not in our route, clear the collection
            SystemsInRoute.ClearCollection();
            //Add the system to our collection
            SystemsInRoute.AddToCollection(sys);
            //Get the scan value of the system from EDSM
            GetSystemValue(sys);
        }

        //Called when a hyperspace jump is started
        public void StartJump(StartJumpEvent.StartJumpEventArgs e)
        {
            InHyperSpace = true;

            SystemInfo sys = new()
            {
                SystemName = e.StarSystem.ToUpperInvariant(),
                SystemAddress = e.SystemAddress,
                StarClass = e.StarClass.ToUpperInvariant()
            };

            LastJumpSystem = sys;

            CurrentSystem.ClearCollection();
            CurrentSystem[0].Bodies.ClearCollection();

            _ = ScanValue.SaveState();

            _ = ScannedBio.SaveState();
        }

        //Called when a hyperspace jump has been completed
        public void OnFSDJump(FSDJumpEvent.FSDJumpEventArgs e)
        {
            //Ship is no longer in hyperspace
            InHyperSpace = false;

            //Check if we have jumped to a system in our route
            SystemInfo sys = SystemsInRoute.FirstOrDefault(x => x.SystemAddress == e.SystemAddress);

            if (sys is not null)
            {
                var index = SystemsInRoute.IndexOf(sys);
                //Just in case the user has jumped to a sys part way through the route
                //we will remove the skipped systems as well
                //Probably isn't required as a new route would be calculated
                //Although in my experience this does not always ping a NavRoute event
                for (int i = index; i > -1; i--)
                {
                    SystemsInRoute.RemoveAtIndexFromCollection(i);
                }
                //Set the current system to the one we just jumped into
                SetCurrentSystem(sys);
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
                //Set the current system to the one we just jumped into
                SetCurrentSystem(sys);

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

                LastJumpSystem = null;

                GetSystemValue(sys);

                SetCurrentSystem(sys);

                return;
            }
            //We must have missed the start the jump so create a new systeminfo and set it as out current system
            sys = new SystemInfo
            {
                SystemName = e.StarSystem.ToUpperInvariant(),
                SystemAddress = e.SystemAddress
            };

            GetSystemValue(sys);

            SetCurrentSystem(sys);
        }
        #endregion

        #region System Population
        //Called when the NavRoute event is fired and the NavRoute.json file has been read
        public void PopulateRoute(NavigationRoute route)
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
            _populatingRoute = true;

            //Clear our collections
            SystemsInRoute.ClearCollection();
            _backupRoute.Clear();

            List<Route> systems = route.Route;

            SystemInfo sys = new(systems[0]);
            //populate the current system
            SetCurrentSystem(sys);

            //populate the systems in route and create the backup
            for (int i = 1; i < count; i++)
            {
                sys = new SystemInfo(systems[i]);
                GetSystemValue(sys);
                SystemsInRoute.AddToCollection(sys);
                _backupRoute.Add(sys);
            }

            _populatingRoute = false;
        }
        //Method to set the current system
        public void SetCurrentSystem(SystemInfo sys)
        {
            //Check we aren't already in the system
            SystemInfo currentSystem = CurrentSystem.FirstOrDefault(x => x.SystemAddress == sys.SystemAddress);
            //If we are in the system, return
            if (currentSystem is not null)
            {
                return;
            }
            //Check we don't have unsold Navigation History
            SystemInfo knownSystem = ScanValue.ScannedSystems.FirstOrDefault(x => x.SystemAddress == sys.SystemAddress);
            //If we do
            if (knownSystem is not null)
            {
                //Make it the current system
                CurrentSystem.ClearCollection();
                CurrentSystem.AddToCollection(knownSystem);
                //Add edsm bodies although they are likely already in there.
                //We will add them just for the rare case something has been discovered since.
                foreach (SystemBody body in sys.Bodies)
                {
                    //Check if we already have the body in the collection
                    SystemBody bodyknown = knownSystem.Bodies.FirstOrDefault(x => x.BodyName == body.BodyName);

                    if (bodyknown == default)
                    {
                        AddBodyToCurrentSystem(false, true, body);
                    }
                }
                return;
            }

            //We aren't aware of the current system so clear the collection and add it
            CurrentSystem.ClearCollection();
            CurrentSystem.AddToCollection(sys);
            GetEDSMBodyCount(sys);
            //If our system information is missing the main star details, get it from EDSM
            if (string.IsNullOrEmpty(sys.StarClass))
            {
                sys.StarClass = GetSystemStarClass(sys.SystemName);
            }
            //If we don't have a scan value for the system, get it from EDSM
            if (sys.SystemValue.Length < 3)
            {
                GetSystemValue(sys);
            }
        }
        //Adds a body to the current system
        public SystemBody AddBodyToCurrentSystem(bool fromDetailedScan, bool fromEDSM, SystemBody bodyToAdd)
        {
            if (bodyToAdd.IsNonBody == true && _appSettings.Value.IgnoreNonBodies)
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
        #endregion

        #region EDSM Methods
        /// <summary>
        /// Contacts EDSM and gets the class of the primary star
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        private static string GetSystemStarClass(string systemName)
        {
            string path = $"https://www.edsm.net/api-v1/system?systemName={HttpUtility.UrlEncode(systemName)}&showPrimaryStar=1";

            string json = GetEDSMJson(path);

            if (string.IsNullOrEmpty(json))
            {
                return string.Empty;
            }
            try
            {
                JObject msg = JObject.Parse(json);

                JObject starinfo = msg["primaryStar"] as JObject;

                string starClass = (string)starinfo["type"];

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
        private void GetEDSMBodyCount(SystemInfo system)
        {
            string path = $"https://www.edsm.net/api-system-v1/bodies?systemName={system.SystemName}";

            string json = GetEDSMJson(path);

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

        private static string[] GeBodyNameFromEDSM(long systemId, long bodyID)
        {
            string[] ret = { "UNKOWN", "-" };
            string path = $"https://www.edsm.net/api-system-v1/bodies?systemId64={systemId}";

            string json = GetEDSMJson(path);

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

                JArray Bodies = msg["bodies"] as JArray;

                foreach (JToken body in Bodies)
                {
                    if ((int)body["bodyId"] == bodyID)
                    {
                        ret[0] = msg["name"].ToObject<string>().ToUpperInvariant();
                        ret[1] = body["name"].ToObject<string>().ToUpperInvariant();
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
        private void GetSystemValue(SystemInfo ret)
        {
            //var ret = new SystemInfo(system);
            //We already have a value
            if (ret.SystemValue.Length > 3)
            {
                return;
            }
            //var path = $"https://www.edsm.net/api-system-v1/estimated-value?systemName={ret.SystemName}";
            string path = $"https://www.edsm.net/api-system-v1/estimated-value?systemId64={ret.SystemAddress}";

            string json = GetEDSMJson(path);

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
                ret.SystemValue = $"{sys.EstimatedValueMapped:N0}";
                ret.EDSMUrl = sys.Url;

                foreach (ValuableBody body in sys.ValuableBodies)
                {
                    SystemBody planet = new();

                    planet.BodyID = body.BodyId;
                    planet.BodyName = body.BodyName;
                    planet.BodyNameLocal = body.BodyName.ToUpperInvariant();
                    planet.DistanceFromArrivalLs = (int)body.Distance;
                    planet.MappedValue = (int)body.ValueMax;
                    planet.WasDiscovered = true;
                    planet.PlanetClass = PlanetClass.EdsmValuableBody;

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
        private static string GetEDSMJson(string path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
                request.Method = "GET";
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("Accept-Encoding", "gzip,deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.Timeout = 5000;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new(dataStream);
                string json = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                return json;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

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
    }
}
