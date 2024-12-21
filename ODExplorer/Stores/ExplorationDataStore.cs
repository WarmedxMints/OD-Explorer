using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.Database;
using ODExplorer.Models;
using ODUtils.APis;
using ODUtils.Database.Interfaces;
using ODUtils.EliteDangerousHelpers;
using ODUtils.EliteDangerousHelpers.GalacticRegions;
using ODUtils.Exobiology;
using ODUtils.Extensions;
using ODUtils.Journal;
using ODUtils.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JournalEntry = ODUtils.Journal.JournalEntry;
using SystemBody = ODUtils.Models.SystemBody;

namespace ODExplorer.Stores
{
    public sealed class ExplorationDataStore : IProcessJournalLogs
    {
        #region Ctor
        public ExplorationDataStore(JournalParserStore parserStore,
                            EdsmApiService edsmApi,
                            IOdToolsDatabaseProvider databaseProvider,
                            NotificationStore notificationStore,
                            SettingsStore settingsStore,
                            ExoData exoData,
                            OrganicCheckListDataStore organicCheckListData)
        {
            this.parserStore = parserStore;
            this.parserStore.OnParserStoreLive += ParserStore_OnParserStoreLive;
            this.edsmApi = edsmApi;
            this.databaseProvider = databaseProvider;
            this.notificationStore = notificationStore;
            this.settingsStore = settingsStore;
            this.exoData = exoData;
            this.organicCheckListData = organicCheckListData;

            if (databaseProvider is OdExplorerDatabaseProvider explorerDatabaseProvider)
            {
                _ = Task.Factory.StartNew(async () =>
                {
                    var pois = await explorerDatabaseProvider.GetAstroPoisAsync();

                    EdAstroPois = pois;
                });
            }
            parserStore.RegisterParser(this);
            parserStore.StatusUpdated += ParserStore_StatusUpdated;
        }
        #endregion

        #region Private Fields
        private readonly JournalParserStore parserStore;
        private readonly EdsmApiService edsmApi;
        private readonly IOdToolsDatabaseProvider databaseProvider;
        private readonly NotificationStore notificationStore;
        private readonly SettingsStore settingsStore;
        private readonly ExoData exoData;
        private readonly OrganicCheckListDataStore organicCheckListData;
        private readonly ConcurrentDictionary<long, StarSystem> _cartoData = [];
        private readonly ConcurrentBag<SystemBody> _organicData = [];
        private readonly Dictionary<long, string> _ignoredSystems = [];

        private bool onFoot;
        private double longitude;
        private double latitude;
        long currentBodyDestinationId;
        #endregion

        #region Public Properties
        public StarSystem? CurrentSystem { get; private set; }
        public string? CurrentSystemName => CurrentSystem?.Name;
        public string? CurrentSystemRegion => CurrentSystem?.Region.Name;
        public SystemBody? CurrentBody { get; private set; }
        public long SelectedBodyId { get; set; }
        public List<StarSystem> Route { get; private set; } = [];
        public OrganicScanItem? CurrentBioItem { get; private set; }
        public List<SystemBody> OrganicScanItems => [.. _organicData];
        public List<EdAstroPoi>? EdAstroPois { get; private set; }
        #endregion

        #region Event Declartions
        public event EventHandler<StarSystem?>? OnCurrentSystemUpdated;
        public event EventHandler<StarSystem?>? OnAllBodiesDiscovered;
        public event EventHandler<SystemBody?>? OnCurrentSystemBodyUpdated;
        public event EventHandler<StarSystem>? OnSystemUpdatedFromEDSM;
        public event EventHandler<List<StarSystem>>? OnRouteUpdated;
        public event EventHandler<SystemBody>? OnBodyUpdated;
        public event EventHandler? OnCartoDataSold;
        public event EventHandler? OnCartoDataLost;
        public event EventHandler<OrganicScanItem>? OnBioDataUpdated;
        public event EventHandler<SystemBody>? OnBodyBiosUpdated;
        public event EventHandler<SystemBody>? OnBodyTargeted;
        public event EventHandler<string>? OnFSDJump;
        public event EventHandler? OnBioDataSold;
        public event EventHandler? OnBioDataLost;
        #endregion

        #region IprocessJournalLogs Implementation

        private readonly List<JournalTypeEnum> cartoHistoricEventsToParse =
        [
            JournalTypeEnum.Location,
            JournalTypeEnum.FSDJump,
            JournalTypeEnum.FSSDiscoveryScan,
            JournalTypeEnum.FSSAllBodiesFound,
            JournalTypeEnum.FSSBodySignals,
            JournalTypeEnum.ScanBaryCentre,
            JournalTypeEnum.SupercruiseEntry,
            JournalTypeEnum.Scan,
            JournalTypeEnum.SAAScanComplete,
            JournalTypeEnum.SAASignalsFound,
            JournalTypeEnum.SellExplorationData,
            JournalTypeEnum.MultiSellExplorationData,
            JournalTypeEnum.Disembark,
            JournalTypeEnum.Embark,
            JournalTypeEnum.Died,
            JournalTypeEnum.ApproachBody,
            JournalTypeEnum.CodexEntry,
            JournalTypeEnum.ScanOrganic,
            JournalTypeEnum.SellOrganicData,
        ];

        public JournalHistoryArgs GetEventsToParse(DateTime defaultAge)
        {
            return new(cartoHistoricEventsToParse, defaultAge, this, ParseHistoryStream);
        }

        public void RunBeforeParsingLogs(int currentCmdrId)
        {
            PopulateIgnoredSystems(currentCmdrId);
        }

        public Task ParseHistoryStream(JournalEntry entry)
        {
            ParseJournalEvent(entry);
            return Task.CompletedTask;
        }

        public void ParseHistory(IEnumerable<JournalEntry> journalEntries, int currentCmdrId)
        {
            PopulateIgnoredSystems(currentCmdrId);

            foreach (var journalEntry in journalEntries)
            {
                ParseJournalEvent(journalEntry);
            }
        }

        public Task ParseHistoryStream(IEnumerable<JournalEntry> journalEntries, int currentCmdrId)
        {
            PopulateIgnoredSystems(currentCmdrId);

            foreach (var journalEntry in journalEntries)
            {
                ParseJournalEvent(journalEntry);
            }
            return Task.CompletedTask;
        }

        public void PopulateIgnoredSystems(int currentCmdrId)
        {
            if (databaseProvider is OdExplorerDatabaseProvider provider)
            {
                _ignoredSystems.Clear();

                var systemsToIgnore = provider.GetIgnoredSystemsDictionary(currentCmdrId);

                foreach (var systems in systemsToIgnore)
                {
                    _ignoredSystems.Add(systems.Key, systems.Value);
                }
            }
        }

        public void ClearData()
        {
            CurrentSystem = null;
            CurrentBody = null;
            CurrentBioItem = null;
            Route.Clear();
            _cartoData.Clear();
            _ignoredSystems.Clear();
            _organicData.Clear();
        }

        public void Dispose()
        {
            parserStore.UnregisterParser(this);
        }

        #region Event Parsering
        public void ParseJournalEvent(JournalEntry e)
        {
            try
            {
                switch (e.EventData)
                {
                    case LocationEvent.LocationEventArgs locationEvt:
                        {
                            var currentSys = UpdateCurrentSystem(new(locationEvt));

                            if (locationEvt.BodyType == BodyType.Planet)
                            {
                                var body = currentSys.SystemBodies.FirstOrDefault(x => x.BodyID == locationEvt.BodyID);

                                body ??= new SystemBody(locationEvt, currentSys);

                                UpdateCurrentBody(body);
                            }
                        }
                        break;
                    case StartJumpEvent.StartJumpEventArgs startJumpEvt:
                        if (parserStore.IsLive && startJumpEvt.JumpType == JumpType.Hyperspace)
                        {
                            OnFSDJump?.Invoke(this, startJumpEvt.StarSystem);
                        }
                        break;
                    case FSDJumpEvent.FSDJumpEventArgs fsdJumpEvent:
                        {
                            var system = UpdateCurrentSystem(new(fsdJumpEvent));

                            if (parserStore.IsLive && settingsStore.NotificationOptions.HasFlag(NotificationOptions.EDSMValuableBody)
                                && system.SystemBodies.Any(x => x.PlanetClass == PlanetClass.EdsmValuableBody))
                            {
                                notificationStore.EDSMValuableBodiesNotification(system);
                            }
                        }
                        break;
                    case FSDTargetEvent.FSDTargetEventArgs fsdTargetEvent:
                        {
                            _cartoData.TryGetValue(fsdTargetEvent.SystemAddress, out var data);

                            data ??= new StarSystem(fsdTargetEvent);

                            if (Route.Contains(data))
                            {
                                //Nothing to do as the target is in the route
                                break;
                            }
                            Route.Clear();
                            Route.Add(data);

                            _cartoData.TryAdd(data.Address, data);

                            Task.Run(async () =>
                            {
                                if (await GetSystemValue(data).ConfigureAwait(true))
                                {
                                    if (parserStore.IsLive)
                                        OnRouteUpdated?.Invoke(this, [data]);
                                }
                            });
                            if (parserStore.IsLive)
                                OnRouteUpdated?.Invoke(this, Route);
                        }
                        break;
                    case NavRouteEvent.NavRouteEventArgs:
                        ParseNavRoute();
                        break;
                    case NavRouteClearEvent.NavRoutClearEventArgs:
                        Route.Clear();
                        if (parserStore.IsLive)
                            OnRouteUpdated?.Invoke(this, Route);
                        break;
                    case SupercruiseEntryEvent.SupercruiseEntryEventArgs:
                        //We have left the body
                        UpdateCurrentBody(null);
                        break;
                    case FSSDiscoveryScanEvent.FSSDiscoveryScanEventArgs fssScan:
                        if (CurrentSystem?.Address == fssScan.SystemAddress)
                        {
                            CurrentSystem.DiscoveredBodyCount = fssScan.BodyCount;
                            CurrentSystem.AllBodiesFound = fssScan.Progress >= 1;
                            TriggerCurrentSystemEventIfLive();
                        }
                        break;
                    case FSSAllBodiesFoundEvent.FSSAllBodiesFoundEventArgs fssAll:
                        if (CurrentSystem?.Address == fssAll.SystemAddress)
                        {
                            CurrentSystem.DiscoveredBodyCount = fssAll.Count;
                            CurrentSystem.AllBodiesFound = true;
                            if (parserStore.IsLive)
                                OnAllBodiesDiscovered?.Invoke(this, CurrentSystem);
                        }
                        break;
                    case FSSBodySignalsEvent.FSSBodySignalsEventArgs fssBodySignals:
                        {
                            if (_cartoData.TryGetValue(fssBodySignals.SystemAddress, out var system))
                            {
                                var body = system.UpdateSignalsFound(fssBodySignals);
                                if (parserStore.IsLive && body.BiologicalSignals > 0 && body.ScanType == ScanType.Detailed)
                                    GetExoBioPredictions(body, fssBodySignals.Timestamp);
                                TriggerBodyEventIfLive(body);
                                break;
                            }
                            //TODO Error if we got this far
                        }
                        break;
                    case ScanEvent.ScanEventArgs scanEvt:
                        {
                            //Ignore nav beacon scans
                            if (scanEvt.ScanType == ScanType.NavBeacon || scanEvt.ScanType == ScanType.NavBeaconDetail)
                                break;
                            //This shouldn't happen but just in case check we aren't scanning something we've already sold
                            if (_cartoData.TryGetValue(scanEvt.SystemAddress, out var system))
                            {
                                var body = system.AddBody(scanEvt,
                                                          settingsStore.SystemGridSetting.ValuableBodyValue,
                                                          settingsStore.SystemGridSetting.ValuableBodyDistance,
                                                          parserStore.Odyssey);
                                if (parserStore.IsLive && body.Item1.BiologicalSignals > 0)
                                    GetExoBioPredictions(body.Item1, e.TimeStamp);
                                TriggerBodyEventIfLive(body.Item1);

                                if (parserStore.IsLive && body.Item1.IsPlanet && body.Item2 == true)
                                    notificationStore.CheckForNotableNotifications(body.Item1);
                                break;
                            }
                            //TODO Error if we got this far
                        }
                        break;
                    case SAAScanCompleteEvent.SAAScanCompleteEventArgs ssaScanComplete:
                        {
                            if (_cartoData.TryGetValue(ssaScanComplete.SystemAddress, out var system))
                            {
                                if (parserStore.IsLive == false)
                                {
                                    var tBody = system.SystemBodies.FirstOrDefault(x => x.BodyID == ssaScanComplete.BodyID);

                                    if (tBody != null && tBody.BiologicalSignals > 0)
                                        GetExoBioPredictions(tBody, ssaScanComplete.Timestamp, false);
                                }
                                var body = system.UpdateBodyFromDSS(ssaScanComplete,
                                                                    settingsStore.SystemGridSetting.ValuableBodyValue,
                                                                    settingsStore.SystemGridSetting.ValuableBodyDistance,
                                                                    parserStore.Odyssey);
                                TriggerBodyEventIfLive(body);
                                break;
                            }
                            //TODO Error if we got this far
                        }
                        break;
                    case SAASignalsFoundEvent.SAASignalsFoundEventArgs ssaSignalsFound:
                        {
                            if (_cartoData.TryGetValue(ssaSignalsFound.SystemAddress, out var system))
                            {
                                var body = system.UpdateSignalsFound(ssaSignalsFound);
                                if (body.BiologicalSignals > 0)
                                {
                                    if (_organicData.Contains(body) == false)
                                    {
                                        _organicData.Add(body);
                                    }
                                    TriggerBodyBiosUpdatedIfLive(body);
                                    break;
                                }
                                TriggerBodyEventIfLive(body);
                                break;
                            }
                        }
                        break;
                    case ScanBaryCentreEvent.ScanBaryCentreEventArgs scanBarycentre:
                        {
                            if (_cartoData.TryGetValue(scanBarycentre.SystemAddress, out var system))
                            {
                                var known = system.SystemBodies.FirstOrDefault(x => x.BodyID == scanBarycentre.BodyID);

                                if (known is null)
                                {
                                    var body = new SystemBody(scanBarycentre, system);
                                    system.SystemBodies.Add(body);
                                }
                            }
                        }
                        break;
                    case SellExplorationDataEvent.SellExplorationDataEventArgs sellCarto:
                        {
                            foreach (string system in sellCarto.Systems)
                            {
                                var known = _cartoData.FirstOrDefault(x => x.Value.Name.Equals(system, StringComparison.OrdinalIgnoreCase)).Value;

                                if (known is null)
                                {
                                    //Ignore if we don't know about it, likely scanned before carto history age setting
                                    continue;
                                }

                                known.MarkBodiesSold();
                            }

                            if (parserStore.IsLive)
                                OnCartoDataSold?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs multiSellCarto:
                        {
                            foreach (var system in multiSellCarto.Discovered)
                            {
                                var known = _cartoData.FirstOrDefault(x => x.Value.Name.Equals(system.SystemName, StringComparison.OrdinalIgnoreCase)).Value;

                                if (known is null)
                                {
                                    //Ignore if we don't know about it, likely scanned before carto history age setting
                                    continue;
                                }

                                known.MarkBodiesSold();
                            }

                            if (parserStore.IsLive)
                                OnCartoDataSold?.Invoke(this, EventArgs.Empty);
                        }
                        break;
                    case DisembarkEvent.DisembarkEventArgs disembark:
                        {
                            onFoot = disembark.OnPlanet;
                            //Check we have the right body assigned
                            if (CurrentBody is not null && CurrentBody.BodyID == disembark.BodyID)
                            {
                                break;
                            }
                            //If we don't, try to find it
                            if (CurrentSystem?.Address == disembark.SystemAddress)
                            {
                                var body = CurrentSystem?.SystemBodies.FirstOrDefault(x => x.BodyID == disembark.BodyID);

                                if (body is not null)
                                    UpdateCurrentBody(body);
                            }
                        }
                        break;
                    case EmbarkEvent.EmbarkEventArgs embark:
                        {
                            onFoot = false;
                        }
                        break;
                    case DiedEvent.DiedEventArgs died:
                        {
                            if (onFoot == false)
                            {
                                var unsold = _cartoData.Where(x => x.Value.UnsoldCount > 0);

                                foreach (var system in unsold)
                                {
                                    system.Value.MarkBodiesLost();
                                }
                                if (parserStore.IsLive)
                                    OnCartoDataLost?.Invoke(this, EventArgs.Empty);
                            }
                            var dataLost = false;
                            foreach (var body in _organicData)
                            {
                                if (body.OrganicScanItems is null)
                                    continue;

                                foreach (var bio in body.OrganicScanItems)
                                {
                                    if (bio.DataState == DataState.Unsold && bio.ScanStage == OrganicScanStage.Analyse)
                                    {
                                        bio.DataState = DataState.Lost;
                                        dataLost = true;
                                    }
                                }
                            }
                            if (dataLost)
                                TriggerBioLostIfLive();
                        }
                        break;
                    case ApproachBodyEvent.ApproachBodyEventArgs approachBody:
                        {
                            //TODO Get system details from EDSM as something has gone wrong
                            CurrentSystem ??= new(new SystemInRoute() { SystemAddress = approachBody.SystemAddress, StarSystem = approachBody.StarSystem });

                            var knownBody = CurrentSystem.SystemBodies.FirstOrDefault(x => x.BodyID == approachBody.BodyID);

                            knownBody ??= new SystemBody(approachBody, CurrentSystem);

                            UpdateCurrentBody(knownBody);
                        }
                        break;
                    case CodexEntryEvent.CodexEntryEventArgs codexEntry:
                        {
                            if (string.Equals("$Codex_SubCategory_Organic_Structures;", codexEntry.SubCategory, StringComparison.OrdinalIgnoreCase) == false)
                                break;

                            var body = CurrentBody;

                            if (body is null || body.BodyID != codexEntry.BodyID)
                            {
                                //TODO Get system details from EDSM as something has gone wrong
                                CurrentSystem ??= new(new SystemInRoute() { SystemAddress = codexEntry.SystemAddress, StarSystem = codexEntry.System });

                                body = CurrentSystem.SystemBodies.FirstOrDefault(x => x.BodyID == codexEntry.BodyID);

                                body ??= new SystemBody(codexEntry, CurrentSystem);
                            }

                            if (body.OrganicScanItems is not null && _organicData.Contains(body))
                            {
                                var genusArray = codexEntry.Name.Split('_').Take(3);
                                var genus = string.Join('_', genusArray);
                                var knownBios = body.OrganicScanItems.Where(x => x.GenusCodex.StartsWith(genus));

                                if (knownBios != null && knownBios.Count() > 1)
                                {
                                    var speciesArray = codexEntry.Name.Split('_').Take(4);
                                    var species = string.Join('_', speciesArray);
                                    foreach (var bio in knownBios)
                                    {
                                        if (bio.SpeciesCodex.StartsWith(species, StringComparison.OrdinalIgnoreCase))
                                        {
                                            bio.UpdateFromCodex(codexEntry);
                                            TriggerBioUpdatedIfLive(bio, true);
                                            continue;
                                        }
                                        bio.ScanStage = OrganicScanStage.Prediction;
                                        OnBioDataUpdated?.Invoke(this, bio);
                                    }
                                    body.UpdateBioMinMaxValue();
                                    break;
                                }

                                if (knownBios != null && knownBios.Count() == 1)
                                {
                                    var bio = knownBios.First();
                                    if (bio.GenusCodex.StartsWith(genus, StringComparison.OrdinalIgnoreCase))
                                    {
                                        bio.UpdateFromCodex(codexEntry);
                                        TriggerBioUpdatedIfLive(bio, true);
                                        break;
                                    }
                                }

                                //Fall back when it wasn't predicted but was found by the DSS
                                var knownBio = body.OrganicScanItems.FirstOrDefault(x => codexEntry.Name_Localised.StartsWith(x.GenusLocalised, StringComparison.OrdinalIgnoreCase));

                                if (knownBio is not null)
                                {
                                    knownBio.UpdateFromCodex(codexEntry);
                                    TriggerBioUpdatedIfLive(knownBio, true);
                                    break;
                                }
                                knownBio = new(codexEntry, body);
                                body.OrganicScanItems.Add(knownBio);
                                if (_organicData.Contains(body) == false)
                                    _organicData.Add(body);
                                TriggerBioUpdatedIfLive(knownBio, true);
                                break;
                            }

                            var newBio = new OrganicScanItem(codexEntry, body);
                            body.OrganicScanItems = [newBio];
                            if (_organicData.Contains(body) == false)
                                _organicData.Add(body);
                            TriggerBioUpdatedIfLive(newBio, true);
                        }
                        break;
                    case ScanOrganicEvent.ScanOrganicEventArgs scanOrganic:
                        {
                            if (string.IsNullOrEmpty(scanOrganic.Genus) || string.IsNullOrEmpty(scanOrganic.Species) || string.IsNullOrEmpty(scanOrganic.Variant))
                                break;

                            var body = CurrentBody;

                            if (body is null || body.BodyID != scanOrganic.Body)
                            {
                                //TODO Get system details from EDSM as something has gone wrong
                                CurrentSystem ??= new(new SystemInRoute() { SystemAddress = scanOrganic.SystemAddress, StarSystem = "Unknown" });

                                body = CurrentSystem.SystemBodies.FirstOrDefault(x => x.BodyID == scanOrganic.Body);

                                body ??= new SystemBody(scanOrganic, CurrentSystem);
                            }

                            if (body.OrganicScanItems is not null && _organicData.Contains(body))
                            {
                                IEnumerable<OrganicScanItem>? knownBio = null;

                                if (scanOrganic.ScanType == OrganicScanStage.Log)
                                {
                                    knownBio = body.OrganicScanItems.Where(x => string.Equals(x.GenusCodex, scanOrganic.Genus, StringComparison.OrdinalIgnoreCase));
                                }
                                if (knownBio is null || !knownBio.Any())
                                {
                                    knownBio = body.OrganicScanItems.Where(x => x.Variants.Exists(v => string.Equals(v.VariantCodex, scanOrganic.Variant, StringComparison.OrdinalIgnoreCase)));
                                }

                                if (knownBio is not null && knownBio.Count() > 1)
                                {
                                    foreach (var bio in knownBio)
                                    {
                                        if (string.Equals(bio.SpeciesCodex, scanOrganic.Species, StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (bio.UpdateFromScan(scanOrganic, longitude, latitude))
                                                TriggerBioUpdatedIfLive(bio);
                                            continue;
                                        }
                                        bio.ScanStage = OrganicScanStage.Prediction;
                                        OnBioDataUpdated?.Invoke(this, bio);
                                    }
                                    body.UpdateBioMinMaxValue();
                                    break;
                                }
                                if (knownBio is not null && knownBio.Count() == 1)
                                {
                                    var bio = knownBio.First();
                                    if (bio.UpdateFromScan(scanOrganic, longitude, latitude))
                                        TriggerBioUpdatedIfLive(bio);
                                    break;
                                }
                                var newBodyBio = new OrganicScanItem(scanOrganic, body, longitude, latitude);
                                body.OrganicScanItems.Add(newBodyBio);
                                if (_organicData.Contains(body) == false)
                                    _organicData.Add(body);
                                TriggerBioUpdatedIfLive(newBodyBio);
                                break;
                            }

                            var newBio = new OrganicScanItem(scanOrganic, body, longitude, latitude);
                            body.OrganicScanItems = [newBio];
                            if (_organicData.Contains(body) == false)
                                _organicData.Add(body);
                            TriggerBioUpdatedIfLive(newBio);
                        }
                        break;
                    case SellOrganicDataEvent.SellOrganicDataEventArgs sellOrganic:
                        {
                            foreach (var organic in sellOrganic.BioData)
                            {
                                foreach (var body in _organicData)
                                {
                                    if (body.OrganicScanItems is null)
                                        continue;

                                    var bio = body.OrganicScanItems.FirstOrDefault(x => x.Variants.Exists(v => string.Equals(v.VariantCodex, organic.Variant, StringComparison.OrdinalIgnoreCase)) && x.ScanStage == OrganicScanStage.Analyse);

                                    if (bio is not null)
                                    {
                                        bio.DataState = DataState.Sold;
                                        continue;
                                    }

                                    bio = body.OrganicScanItems.FirstOrDefault(x => string.Equals(x.SpeciesCodex, organic.Species, StringComparison.OrdinalIgnoreCase) && x.ScanStage == OrganicScanStage.Analyse);

                                    if (bio is not null)
                                    {
                                        bio.DataState = DataState.Sold;
                                        continue;
                                    }
                                }
                            }

                            TriggerBioSoldIfLive();
                        }
                        break;
                }
            }
            catch (NullReferenceException ex)
            {
                App.Logger.Error(ex, "Null Reference parsing journal logs");
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Exception parsing journal logs");
            }
        }
        #endregion

        #endregion

        #region Bio Prediction Method
        private void GetExoBioPredictions(SystemBody body, DateTime timeStamp, bool notifications = true, bool forceParse = false)
        {
            if (forceParse == false && body.HasExoPredictions)
                return;

            var distance = 0d;

            //Electricae_Radialem needs to be near nebula
            if (body.PlanetClass == PlanetClass.IcyBody)
            {
                distance = EdAstroPois?.Where(x => x.Type == ODUtils.Models.EdAstro.EDAstroType.Nebulae)
                    .Min(x => x.SystemPosition.DistanceFrom(body.Owner.Position)) ?? 0d;
            }
            List<StarType> stars = body.GetGoverningStar();

            if (stars.Count == 0)
            {
                stars = body.Owner.SystemBodies.Where(x => x.IsStar).Select(x => x.StarType).ToList();
            }

            var parentStars = body.GetParentStars();

            var region = ODUtils.Helpers.EnumUtility.GetEnumValueFromDescription<GalacticRegions>(body.Owner.Region.Name);
            var planet = new ExoPlanet(body.PlanetClass,
                                        body.Atmosphere,
                                        body.AtmosphereType,
                                        body.AtmosphereComposition ?? [],
                                        body.Volcanism,
                                        body.SurfaceGravity,
                                        body.SurfaceTemp,
                                        body.SurfacePressure / 101325, //convert to atm
                                        body.DistanceFromArrivalLs,
                                        body.OrbitalPeriod,
                                        body.Materials,
                                        stars,
                                        parentStars,
                                        region,
                                        timeStamp,
                                        distance,
                                        body.BiologicalSignals);

            var predictions = exoData.GetPredictions(planet);

            if (predictions.Count == 0)
                return;

            body.HasExoPredictions = true;
            Dictionary<string, bool> newCodexEntries = [];
            Dictionary<string, bool> newSpeciesEntries = [];

            foreach (var prediction in predictions.Values)
            {
                body.OrganicScanItems ??= [];

                foreach (var pred in prediction)
                {
                    pred.IsNewSpecies = organicCheckListData.IsNewSpecies(pred.SpeciesCodex);

                    if (pred.IsNewSpecies)
                    {
                        newSpeciesEntries.TryAdd($"{pred.GenusEnglishName} {pred.SpeciesEnglishName}", false);
                    }

                    foreach (var variant in pred.Variants)
                    {
                        variant.NewCodexEntry = organicCheckListData.IsNewCodex(variant.VariantCodex);

                        if (variant.NewCodexEntry)
                        {
                            newCodexEntries.TryAdd($"{variant.EnglishName}", pred.IsNewSpecies);
                        }
                    }

                    var known = body.OrganicScanItems.FirstOrDefault(x => x.SpeciesCodex == pred.SpeciesCodex && x.ScanStage != OrganicScanStage.Prediction);
                    if (known is not null)
                    {
                        known.UpdateFromPrediction(pred);
                        continue;
                    }
                    if (body.DssScanned == false)
                    {
                        body.OrganicScanItems.Add(new(pred, body));
                        continue;
                    }
                    //fallback to genus values for situations where there app has been restarted after a DSS scan
                    //but before the items have be scanned on foot
                    known = body.OrganicScanItems.FirstOrDefault(x => x.GenusCodex == pred.GenusCodex);
                    if (known is not null)
                    {
                        var multiple = body.OrganicScanItems.Where(x => x.GenusCodex == pred.GenusCodex).Count() > 1;

                        known.UpdateFromPrediction(pred);

                        if (!multiple)
                            continue;

                        known.ScanStage = OrganicScanStage.Prediction;
                        foreach (var variant in known.Variants)
                        {
                            variant.Confirmed = false;
                        }
                        body.OrganicScanItems.Add(new(pred, body));
                        continue;
                    }
                    body.OrganicScanItems.Add(new(pred, body));
                }
            }
            body.UpdateBioMinMaxValue();

            if (notifications && parserStore.IsLive &&
                settingsStore.NotificationOptions.HasFlag(NotificationOptions.ValuableBioPlanet)
               && body.MinExoValue >= settingsStore.SystemGridSetting.ExoValuableBodyValue)
            {
                var min = body.MinExoValue;
                var max = body.MaxExoValue;
                var valueString = string.Empty;

                valueString = min == max ? $"{max.FormatNumber()}" : $"{min.FormatNumber()} - {max.FormatNumber()}";
                var countString = body.BiologicalSignals > 1 ? $"{body.BiologicalSignals} Signals" : $"{body.BiologicalSignals} Signal";

                notificationStore.ShowHighValueExoBodyNotification(body.BodyName, valueString, countString);
            }

            if (notifications && parserStore.IsLive
                && newCodexEntries.Count > 0
                && settingsStore.NotificationOptions.HasFlag(NotificationOptions.NewBioCodexEntry))
            {
                notificationStore.ShowNewCodexEntriesNotification(body.BodyName, newCodexEntries, CurrentSystemRegion);
            }

            if (notifications && parserStore.IsLive &&
                newSpeciesEntries.Count > 0
                && settingsStore.NotificationOptions.HasFlag(NotificationOptions.NewBioSpecies))
            {
                notificationStore.ShowNewSpeciesEntriesNotification(body.BodyName, newSpeciesEntries, CurrentSystemRegion);
            }
            TriggerBodyBiosUpdatedIfLive(body);
        }
        #endregion

        #region Data Value Methods
        public List<StarSystem> GetUnsoldCartoSystems()
        {
            var systems = _cartoData.Values.ToList();

            var ret = systems.Where(x => _ignoredSystems.ContainsKey(x.Address) == false)
                             .Where(x => x.UnsoldCount > 0)
                             .OrderBy(x => x.Name)
                             .ToList();
            return ret;
        }

        public List<StarSystem> GetSoldCartoSystems()
        {
            var systems = _cartoData.Values.ToList();

            var ret = systems.Where(x => _ignoredSystems.ContainsKey(x.Address) == false)
                             .Where(x => x.SoldCount > 0)
                             .OrderBy(x => x.Name)
                             .ToList();
            return ret;
        }

        public List<StarSystem> GetLostCartoSystems()
        {
            var systems = _cartoData.Values.ToList();

            var ret = systems.Where(x => _ignoredSystems.ContainsKey(x.Address) == false)
                             .Where(x => x.LostCount > 0)
                             .OrderBy(x => x.Name)
                             .ToList();
            return ret;
        }

        internal string GetUnsoldExoValueString()
        {
            long value = 0;

            foreach (var Body in OrganicScanItems)
            {
                if (Body.OrganicScanItems is null)
                    continue;

                value += Body.OrganicScanItems.Where(x => x.ScanStage == OrganicScanStage.Analyse && x.DataState == DataState.Unsold).Sum(x => x.TotalValue);
            }

            return value.ToString("N0");
        }
        #endregion

        #region Event Methods
        private void TriggerBodyBiosUpdatedIfLive(SystemBody body)
        {
            if (parserStore.IsLive == false)
                return;

            OnBodyBiosUpdated?.Invoke(this, body);
        }



        private void TriggerCurrentSystemEventIfLive()
        {
            if (parserStore.IsLive == false)
                return;

            OnCurrentSystemUpdated?.Invoke(this, CurrentSystem);
        }

        private void TriggerBodyEventIfLive(SystemBody body)
        {
            if (parserStore.IsLive == false || body.UpdatedFromScanEvent == false)
                return;

            if (settingsStore.NotificationOptions.HasFlag(NotificationOptions.WorthMapping) && body.Status == DiscoveryStatus.WorthMapping)
                notificationStore.ShowWorthMappingNotification(body);

            OnBodyUpdated?.Invoke(this, body);
        }


        private void ParserStore_StatusUpdated(object? sender, StatusFileEvent e)
        {
            CheckCurrentBody(e);
            latitude = e.Latitude;
            longitude = e.Longitude;

            if (latitude == 0 && longitude == 0)
                return;

            if (CurrentBioItem == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(e.BodyName))
            {
                CurrentBioItem = null;
                return;
            }

            if (CurrentBioItem.Info is null || CurrentBioItem.ScanStage <= OrganicScanStage.Codex)
                return;

            var colonyRange = CurrentBioItem.Info.ColonyRange;

            foreach (var scan in CurrentBioItem.ScanLocations)
            {
                scan.Distance = BodyHelpers.DistanceBetweenLongLats(scan.Latitude, scan.Longitude, e.Latitude, e.Longitude, e.PlanetRadius);

                var newState = scan.Distance > colonyRange ? ScanNotificationState.FarEnough : ScanNotificationState.TooClose;

                scan.DistanceState = newState;
            }

            if (settingsStore.NotificationOptions.HasFlag(NotificationOptions.DistanceFromBio) == false)
                return;

            var locations = CurrentBioItem.ScanLocations.Where(x => x.ScanStage > OrganicScanStage.Codex);

            if (locations.All(x => x.HasPos && x.DistanceState == ScanNotificationState.FarEnough)
                && CurrentBioItem.NotificationState == ScanNotificationState.TooClose)
            {
                CurrentBioItem.NotificationState = ScanNotificationState.FarEnough;
                notificationStore.ShowExoBioNotification(CurrentBioItem, "Minimum Distance Travelled");
                return;
            }

            if (locations.Any(x => x.HasPos && x.DistanceState == ScanNotificationState.TooClose)
                && CurrentBioItem.NotificationState == ScanNotificationState.FarEnough)
            {
                CurrentBioItem.NotificationState = ScanNotificationState.TooClose;
                notificationStore.ShowExoBioNotification(CurrentBioItem, "Moved Too Close To Scans");
            }
        }

        private void TriggerBioUpdatedIfLive(OrganicScanItem knownBio, bool fromCodex = false)
        {
            if (parserStore.IsLive == false)
                return;

            if (!fromCodex)
                SetCurrentBio(knownBio);

            OnBioDataUpdated?.Invoke(this, knownBio);

            if (fromCodex && onFoot || knownBio.Info is null)
                return;
            if (settingsStore.NotificationOptions.HasFlag(NotificationOptions.NewBioScanned))
            {
                notificationStore.ShowExoBioNotification(knownBio, "");
            }

        }

        private void TriggerBioSoldIfLive()
        {
            if (parserStore.IsLive == false)
                return;

            OnBioDataSold?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerBioLostIfLive()
        {
            if (parserStore.IsLive == false)
                return;

            OnBioDataLost?.Invoke(this, EventArgs.Empty);
        }

        private void ParserStore_OnParserStoreLive(object? sender, bool e)
        {
            if (e && CurrentSystem != null)
            {
                foreach (var body in CurrentSystem.SystemBodies)
                {
                    if (body.BiologicalSignals > 0)
                    {
                        GetExoBioPredictions(body, DateTime.UtcNow, false);
                    }
                }
                Task.Run(async () =>
                {

                    var starUpdate = CurrentSystem.StarType == StarType.Unknown && await UpdateSystemStarClass(CurrentSystem).ConfigureAwait(true);
                    var valueUpdate = CurrentSystem.EstimatedValue == 0 && await GetSystemValue(CurrentSystem).ConfigureAwait(true);
                    var countUpdate = CurrentSystem.BodyCount == 0 && await UpdateKnownBodyCount(CurrentSystem).ConfigureAwait(true);
                    if (starUpdate || valueUpdate || countUpdate)
                        OnSystemUpdatedFromEDSM?.Invoke(this, CurrentSystem);
                });
            }
        }
        #endregion

        #region System Methods
        private void CheckCurrentBody(StatusFileEvent e)
        {
            if (currentBodyDestinationId != e.Destination.Body && string.IsNullOrEmpty(e.BodyName))
            {
                var known = CurrentSystem?.SystemBodies.FirstOrDefault(x => x.BodyID == e.Destination.Body);

                if (known != null)
                {
                    currentBodyDestinationId = known.BodyID;
                    OnBodyTargeted?.Invoke(this, known);
                    return;
                }
            }

            if (string.IsNullOrEmpty(e.BodyName) == false)
            {
                var known = CurrentSystem?.SystemBodies.FirstOrDefault(x => x.BodyName.Equals(e.BodyName));

                if (known != null && known.BodyID != currentBodyDestinationId)
                {
                    currentBodyDestinationId = known.BodyID;
                    OnBodyTargeted?.Invoke(this, known);
                    return;
                }
            }
        }



        private void SetCurrentBio(OrganicScanItem scanItem)
        {
            if (scanItem.ScanStage == OrganicScanStage.Analyse)
            {
                CurrentBioItem = null;
                return;
            }
            //If we've scanned something new without finishing the last item
            //then clear the scan locations
            if (CurrentBioItem != scanItem && CurrentBioItem?.ScanStage < OrganicScanStage.Analyse)
            {
                CurrentBioItem.ScanLocations.RemoveAll(x => x.ScanStage >= OrganicScanStage.Codex);
                CurrentBioItem.NotificationState = ScanNotificationState.TooClose;
            }
            CurrentBioItem = scanItem;
        }


        private void ParseNavRoute()
        {
            Route.Clear();

            var route = parserStore.GetNavRoute();

            if (route is null)
                return;

            foreach (var system in route.Route)
            {
                if (system is null)
                    continue;

                if (CurrentSystem is not null && CurrentSystem?.Address == system.SystemAddress)
                {
                    CurrentSystem.StarType = system.StarClass;
                    CurrentSystem.Name = system.StarSystem;
                    CurrentSystem.Position = new(system.StarPos);
                    //TriggerCurrentSystemEventIfLive();
                    continue;
                }

                if (_cartoData.TryGetValue(system.SystemAddress, out var data))
                {
                    data.StarType = system.StarClass;
                    data.Name = system.StarSystem;
                    data.Position = new(system.StarPos);
                    Route.Add(data);
                    continue;
                }

                var sys = new StarSystem(system);
                _cartoData.TryAdd(sys.Address, sys);
                Route.Add(sys);
            }

            OnRouteUpdated?.Invoke(this, Route);

            foreach (var system in Route)
            {
                Task.Run(async () =>
                {
                    if (await GetSystemValue(system).ConfigureAwait(true))
                    {
                        OnSystemUpdatedFromEDSM?.Invoke(this, system);
                    }
                });
            }
        }

        private StarSystem CheckIfSystemKnown(StarSystem system)
        {
            if (_cartoData.TryGetValue(system.Address, out var value))
            {
                //Update position and region
                value.Position = new(system.Position.X, system.Position.Y, system.Position.Z);
                value.Region = RegionMap.FindRegion(system.Position.X, system.Position.Y, system.Position.Z);
                return value;
            }
            _cartoData.TryAdd(system.Address, system);
            return system;
        }

        private StarSystem UpdateCurrentSystem(StarSystem newSystem)
        {
            var system = CheckIfSystemKnown(newSystem);

            var known = Route.FirstOrDefault(x => x.Address == system.Address);

            if (known != null)
            {
                var index = Route.IndexOf(known);
                Route.RemoveRange(0, index + 1);
                CurrentSystem = known;
                Task.Run(async () => { await UpdateKnownBodyCount(CurrentSystem).ConfigureAwait(true); OnSystemUpdatedFromEDSM?.Invoke(this, CurrentSystem); }).ConfigureAwait(true);
                OnCurrentSystemUpdated?.Invoke(this, CurrentSystem);
                OnRouteUpdated?.Invoke(this, Route);
                return CurrentSystem;
            }

            CurrentSystem = system;

            if (parserStore.IsLive == false)
                return CurrentSystem;

            OnCurrentSystemUpdated?.Invoke(this, CurrentSystem);

            Task.Run(async () =>
            {
                var starUpdate = CurrentSystem.StarType == StarType.Unknown && await UpdateSystemStarClass(system).ConfigureAwait(true);
                var valueUpdate = CurrentSystem.EstimatedValue == 0 && await GetSystemValue(system).ConfigureAwait(true);
                var countUpdate = CurrentSystem.BodyCount == 0 && await UpdateKnownBodyCount(system).ConfigureAwait(true);
                if (starUpdate || valueUpdate || countUpdate)
                    OnSystemUpdatedFromEDSM?.Invoke(this, CurrentSystem);
            });

            return CurrentSystem;
        }

        private void UpdateCurrentBody(SystemBody? body)
        {
            CurrentBody = body;

            if (parserStore.IsLive)
                OnCurrentSystemBodyUpdated?.Invoke(this, CurrentBody);
        }

        private async Task<bool> UpdateKnownBodyCount(StarSystem system)
        {
            var count = await edsmApi.GetBodyCountAsync(system.Address).ConfigureAwait(true);

            var ret = system.BodyCount != count;
            system.BodyCount = count;
            if (count > 0)
            {
                system.IsKnownToEDSM = true;
            }
            return ret;
        }

        private async Task<bool> UpdateSystemStarClass(StarSystem system)
        {
            if (parserStore.IsLive == false || system == null)
                return false;

            bool ret = false;

            var starclass = await edsmApi.GetPrimaryStarClassAsync(system.Name).ConfigureAwait(true);

            if (starclass != StarType.Unknown)
            {
                system.StarType = starclass;
                ret = true;
            }

            return ret;
        }

        private async Task<bool> GetSystemValue(StarSystem system)
        {
            var value = await edsmApi.GetSystemValueAsync(system.Name).ConfigureAwait(true);
            bool ret = false;

            if (value is not null)
            {
                if (value.ValuableBodies is not null)
                {
                    foreach (var body in value.ValuableBodies)
                    {
                        bool systemKnown = system.SystemBodies.FirstOrDefault(x => string.Equals(x.BodyName, body.BodyName, StringComparison.OrdinalIgnoreCase)) != default;

                        if (systemKnown)
                        {
                            continue;
                        }

                        SystemBody planet = new(body, system);
                        system.SystemBodies.Add(planet);
                    }
                }
                system.EdsmUrl = value?.Url ?? string.Empty;
                system.IsKnownToEDSM = true;
                system.EstimatedValue = value?.EstimatedValueMapped ?? 0;
                ret = true;
            }

            return ret;
        }

        public void ForceParsePredictions()
        {
            if (CurrentSystem != null)
                foreach (var body in CurrentSystem.SystemBodies)
                {
                    if (body.BiologicalSignals > 0)
                    {
                        body.OrganicScanItems?.Clear();
                        GetExoBioPredictions(body, DateTime.UtcNow, false, true);
                        OnBodyUpdated?.Invoke(this, body);
                    }
                }
        }
        #endregion
    }
}
