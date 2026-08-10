using EliteJournalReader;
using ODExplorer.Models;
using ODUtils.Models;
using ODUtils.Spansh;
using System;
using System.Collections.Generic;

namespace ODExplorer.Stores
{
    /// <summary>
    /// Cross references an imported Spansh route against the commander's own journal history so route entries
    /// can show what has already been done. Owns no data of its own, it only reads what
    /// <see cref="ExplorationDataStore"/> has already built from the journal logs.
    /// </summary>
    public sealed class SpanshRouteProgressStore : IDisposable
    {
        #region Constructor
        public SpanshRouteProgressStore(ExplorationDataStore explorationData, JournalParserStore parserStore)
        {
            this.explorationData = explorationData;
            this.parserStore = parserStore;

            this.explorationData.OnBioDataUpdated += ExplorationData_OnBioDataUpdated;
            this.explorationData.OnBodyUpdated += ExplorationData_OnBodyUpdated;
            this.explorationData.OnBodyBiosUpdated += ExplorationData_OnBodyUpdated;
            this.explorationData.OnBioDataSold += ExplorationData_OnDataStateChanged;
            this.explorationData.OnBioDataLost += ExplorationData_OnDataStateChanged;
            this.explorationData.OnCartoDataSold += ExplorationData_OnDataStateChanged;
            this.explorationData.OnCartoDataLost += ExplorationData_OnDataStateChanged;
            this.parserStore.OnParserStoreLive += ParserStore_OnParserStoreLive;
        }
        #endregion

        #region Private Fields
        private readonly ExplorationDataStore explorationData;
        private readonly JournalParserStore parserStore;
        private readonly object indexLock = new();

        //Lookup accelerators only. The systems and bodies they hold are the live objects, so the states read
        //off them are always current, the index only needs rebuilding when new systems or bodies appear.
        private Dictionary<string, StarSystem>? systemsByName;
        private Dictionary<string, List<SystemBody>>? organicBodiesBySystem;
        #endregion

        #region Events
        /// <summary>
        /// Raised with the name of the system whose progress changed, or null when everything should be
        /// re-evaluated.
        /// </summary>
        public event EventHandler<string?>? OnProgressChanged;
        #endregion

        #region Public Methods
        /// <summary>
        /// Progress for a single route row. For <see cref="CsvType.Exobiology"/> a row is one body and one
        /// species, for the body routes it is one body, and <paramref name="species"/> is ignored.
        /// </summary>
        public SpanshProgressState GetBodyState(CsvType csvType, string? systemName, string? bodyName, string? species)
        {
            if (string.IsNullOrWhiteSpace(systemName) || string.IsNullOrWhiteSpace(bodyName))
            {
                return SpanshProgressState.None;
            }

            try
            {
                return csvType switch
                {
                    CsvType.Exobiology => GetExobiologyState(systemName, bodyName, species),
                    CsvType.RoadToRiches or CsvType.WorldTypeRoute => GetCartoBodyState(systemName, bodyName),
                    _ => SpanshProgressState.None,
                };
            }
            catch (Exception ex)
            {
                //The journal parser mutates these collections on its own thread, so a read can collide with a
                //scan being added. Nothing is lost, the next event refreshes the row.
                App.Logger.Error(ex, "Exception resolving Spansh route progress");
                return SpanshProgressState.None;
            }
        }

        /// <summary>
        /// Progress for a whole system, rolled up from the states of its rows. A system claims a state only once
        /// every one of its rows has reached it, so a system still holding unfinished entries reads as started
        /// rather than done.
        /// </summary>
        public SpanshProgressState RollUpSystemState(IEnumerable<SpanshProgressState> rowStates)
        {
            var lowest = SpanshProgressState.Sold;
            var highest = SpanshProgressState.None;
            var hasRows = false;

            foreach (var state in rowStates)
            {
                hasRows = true;

                if (state < lowest)
                {
                    lowest = state;
                }

                if (state > highest)
                {
                    highest = state;
                }
            }

            //A jump route has no rows to roll up, so it is never coloured.
            if (hasRows == false || highest == SpanshProgressState.None)
            {
                return SpanshProgressState.None;
            }

            //A system only claims a state once every one of its rows has reached it, so the weakest row decides.
            //Anything less than that is simply "started", which is what the partial shade is for.
            return lowest switch
            {
                SpanshProgressState.Sold => SpanshProgressState.Sold,
                SpanshProgressState.Gathered => SpanshProgressState.Gathered,
                _ => SpanshProgressState.Scanned,
            };
        }

        public void Dispose()
        {
            explorationData.OnBioDataUpdated -= ExplorationData_OnBioDataUpdated;
            explorationData.OnBodyUpdated -= ExplorationData_OnBodyUpdated;
            explorationData.OnBodyBiosUpdated -= ExplorationData_OnBodyUpdated;
            explorationData.OnBioDataSold -= ExplorationData_OnDataStateChanged;
            explorationData.OnBioDataLost -= ExplorationData_OnDataStateChanged;
            explorationData.OnCartoDataSold -= ExplorationData_OnDataStateChanged;
            explorationData.OnCartoDataLost -= ExplorationData_OnDataStateChanged;
            parserStore.OnParserStoreLive -= ParserStore_OnParserStoreLive;
        }
        #endregion

        #region State Resolution
        private SpanshProgressState GetExobiologyState(string systemName, string bodyName, string? species)
        {
            var state = SpanshProgressState.None;

            foreach (var body in GetBodiesWithOrganics(systemName))
            {
                if (BodyNameMatches(systemName, bodyName, body.BodyName) == false)
                {
                    continue;
                }

                if (body.OrganicScanItems is null)
                {
                    continue;
                }

                //The prediction engine can hold more than one item for the same species on a body, with the
                //ones it decided against demoted back to a prediction, so take the best of them.
                foreach (var organic in body.OrganicScanItems)
                {
                    if (SpeciesMatches(species, organic) == false)
                    {
                        continue;
                    }

                    var organicState = GetOrganicState(organic);

                    if (organicState > state)
                    {
                        state = organicState;
                    }
                }
            }

            return state;
        }

        private static SpanshProgressState GetOrganicState(OrganicScanItem organic)
        {
            if (organic.DataState == DataState.Sold)
            {
                return SpanshProgressState.Sold;
            }

            //Data lost to a death reads as still to do.
            if (organic.DataState == DataState.Lost)
            {
                return SpanshProgressState.None;
            }

            return organic.ScanStage switch
            {
                OrganicScanStage.Analyse => SpanshProgressState.Gathered,
                OrganicScanStage.Sample or OrganicScanStage.Log => SpanshProgressState.Scanned,
                //A prediction or a codex entry is not something the commander has sampled here.
                _ => SpanshProgressState.None,
            };
        }

        private SpanshProgressState GetCartoBodyState(string systemName, string bodyName)
        {
            var system = GetSystem(systemName);

            if (system?.SystemBodies is null)
            {
                return SpanshProgressState.None;
            }

            foreach (var body in system.SystemBodies)
            {
                //EDSM valuable bodies are placeholders the commander has not scanned.
                if (body.PlanetClass == PlanetClass.EdsmValuableBody)
                {
                    continue;
                }

                if (BodyNameMatches(systemName, bodyName, body.BodyName) == false)
                {
                    continue;
                }

                return body.BodyDataState switch
                {
                    DataState.Sold => SpanshProgressState.Sold,
                    DataState.Lost => SpanshProgressState.None,
                    _ => body.DssScanned ? SpanshProgressState.Gathered : SpanshProgressState.Scanned,
                };
            }

            return SpanshProgressState.None;
        }
        #endregion

        #region Name Matching
        private static bool BodyNameMatches(string systemName, string routeBodyName, string? knownBodyName)
        {
            if (string.IsNullOrWhiteSpace(knownBodyName))
            {
                return false;
            }

            var route = routeBodyName.Trim();
            var known = knownBodyName.Trim();

            if (string.Equals(route, known, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            //Spansh gives the full body name, journal logs agree, but compare the short forms too in case a
            //route ever arrives with the system prefix already stripped.
            return string.Equals(StripSystemName(systemName, route),
                                 StripSystemName(systemName, known),
                                 StringComparison.OrdinalIgnoreCase);
        }

        private static string StripSystemName(string systemName, string bodyName)
        {
            if (string.IsNullOrEmpty(systemName))
            {
                return bodyName;
            }

            return bodyName.StartsWith(systemName, StringComparison.OrdinalIgnoreCase) && bodyName.Length > systemName.Length
                ? bodyName.Remove(0, systemName.Length + 1)
                : bodyName;
        }

        private static bool SpeciesMatches(string? species, OrganicScanItem organic)
        {
            if (string.IsNullOrWhiteSpace(species))
            {
                return false;
            }

            var name = species.Trim();
            var genus = GetGenusName(organic);
            var english = $"{genus} {GetSpeciesName(organic)}".Trim();

            if (string.IsNullOrWhiteSpace(english) == false
                && string.Equals(english, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(organic.SpeciesLocalised) == false
                && string.Equals(organic.SpeciesLocalised, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            //A route filtered by genus alone only carries the genus name.
            return string.IsNullOrWhiteSpace(genus) == false
                && string.Equals(genus, name, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetGenusName(OrganicScanItem organic)
        {
            return string.IsNullOrEmpty(organic.GenusEnglish) ? organic.GenusLocalised : organic.GenusEnglish;
        }

        private static string GetSpeciesName(OrganicScanItem organic)
        {
            return string.IsNullOrEmpty(organic.SpeciesEnglish) ? organic.SpeciesLocalised : organic.SpeciesEnglish;
        }
        #endregion

        #region Index
        private StarSystem? GetSystem(string systemName)
        {
            lock (indexLock)
            {
                systemsByName ??= explorationData.GetCartoSystemsByName();

                return systemsByName.TryGetValue(systemName, out var system) ? system : null;
            }
        }

        private List<SystemBody> GetBodiesWithOrganics(string systemName)
        {
            List<SystemBody> bodies = [];

            lock (indexLock)
            {
                organicBodiesBySystem ??= BuildOrganicBodyIndex();

                if (organicBodiesBySystem.TryGetValue(systemName, out var known))
                {
                    bodies.AddRange(known);
                }
            }

            //Fall back to the system's own bodies for anything the organic list has not picked up.
            var system = GetSystem(systemName);

            if (system?.SystemBodies is null)
            {
                return bodies;
            }

            foreach (var body in system.SystemBodies)
            {
                if (body.OrganicScanItems is not null && bodies.Contains(body) == false)
                {
                    bodies.Add(body);
                }
            }

            return bodies;
        }

        private Dictionary<string, List<SystemBody>> BuildOrganicBodyIndex()
        {
            Dictionary<string, List<SystemBody>> index = new(StringComparer.OrdinalIgnoreCase);

            foreach (var body in explorationData.OrganicScanItems)
            {
                var systemName = body.Owner?.Name;

                if (string.IsNullOrEmpty(systemName))
                {
                    continue;
                }

                if (index.TryGetValue(systemName, out var bodies) == false)
                {
                    bodies = [];
                    index.Add(systemName, bodies);
                }

                if (bodies.Contains(body) == false)
                {
                    bodies.Add(body);
                }
            }

            return index;
        }

        private void InvalidateIndex()
        {
            lock (indexLock)
            {
                systemsByName = null;
                organicBodiesBySystem = null;
            }
        }
        #endregion

        #region Event Handlers
        private void ExplorationData_OnBioDataUpdated(object? sender, OrganicScanItem e)
        {
            InvalidateIndex();
            RaiseProgressChanged(e?.Body?.Owner?.Name);
        }

        private void ExplorationData_OnBodyUpdated(object? sender, SystemBody e)
        {
            InvalidateIndex();
            RaiseProgressChanged(e?.Owner?.Name);
        }

        private void ExplorationData_OnDataStateChanged(object? sender, EventArgs e)
        {
            //Selling and dying are not tied to a single system, so everything has to be re-evaluated.
            InvalidateIndex();
            RaiseProgressChanged(null);
        }

        private void ParserStore_OnParserStoreLive(object? sender, bool e)
        {
            InvalidateIndex();
            RaiseProgressChanged(null);
        }

        private void RaiseProgressChanged(string? systemName)
        {
            OnProgressChanged?.Invoke(this, systemName);
        }
        #endregion
    }
}
