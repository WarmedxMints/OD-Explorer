using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.Models;
using ODUtils.EliteDangerousHelpers.GalacticRegions;
using ODUtils.Exobiology;
using ODUtils.Journal;
using ODUtils.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Windows.Forms.AxHost;

namespace ODExplorer.Stores
{
    public sealed class OrganicCheckListDataStore : IProcessJournalLogs
    {
        public OrganicCheckListDataStore(JournalParserStore parserStore, ExoData exoData)
        {
            this.parserStore = parserStore;
            this.exoData = exoData;
            this.exoData.Initialise();
            this.parserStore.OnParserStoreLive += ParserStore_OnParserStoreLive;
            parserStore.RegisterParser(this);
            this.parserStore = parserStore;

            Task.Factory.StartNew(BuildDictionary);
        }

        private readonly Dictionary<GalacticRegions, List<string>> codexEntries = [];
        private readonly Dictionary<GalacticRegions, List<string>> speciesEntries = [];
        private readonly JournalParserStore parserStore;
        private readonly ExoData exoData;
        //The day before odyssey release
        private readonly DateTime historicAge = new(2021, 05, 18);
        private GalacticRegions currentRegion;

        public event EventHandler? OnOrganicScanDetailsUpdated;
        public event EventHandler<string>? OnSpeciesUpdated;

        public readonly Dictionary<string, List<OrganicChecklistItem>> OrganicScanItems = [];

        private readonly List<JournalTypeEnum> historicEventsToParse =
        [
            JournalTypeEnum.ScanOrganic,
            JournalTypeEnum.SellOrganicData,
            JournalTypeEnum.FSDJump,
            JournalTypeEnum.CodexEntry,
            JournalTypeEnum.Died
        ];

        private void BuildDictionary()
        {
            OrganicScanItems.Clear();

            var genus = exoData.AllGenus;
            var regions = Enum.GetValues(typeof(GalacticRegions));
            foreach (var item in genus)
            {
                var speciesToAdd = new List<OrganicChecklistItem>();

                foreach (var value in item.Species)
                {
                    var species = new OrganicChecklistItem(value.SpeciesCodex, value.SpeciesName);

                    foreach (var area in regions)
                    {
                        if (area is not GalacticRegions region || region is GalacticRegions.Unknown)
                        {
                            continue;
                        }

                        var available = value.Regions.Contains(region);
                        species.AddRegion(region, available ? OrganicScanState.None : OrganicScanState.Unavailable);

                        if (available == false)
                            continue;

                        foreach (var variant in value.Variants)
                        {
                            species.AddVariant(variant.Codex, variant.Colour.ToString(), region, OrganicScanState.None);
                        }
                    }
                    speciesToAdd.Add(species);
                }
                OrganicScanItems.TryAdd(item.Codex, speciesToAdd);
            }

            OrganicScanItems.TryAdd("Other", []);
        }

        private void AddBioScan(ScanOrganicEvent.ScanOrganicEventArgs scanOrganic)
        {
            if (string.IsNullOrEmpty(scanOrganic.Genus) || string.IsNullOrEmpty(scanOrganic.Species) || string.IsNullOrEmpty(scanOrganic.Variant))
                return;

            var state = scanOrganic.ScanType == OrganicScanStage.Analyse ? OrganicScanState.Analysed : OrganicScanState.Discovered;

            if (OrganicScanItems.TryGetValue(scanOrganic.Genus, out var items))
            {
                var item = items.FirstOrDefault(x => x.SpeciesCodex == scanOrganic.Species);

                if (item is null)
                    return;

                item.AddRegion(currentRegion, state);
                if (string.IsNullOrEmpty(scanOrganic.Variant) == false)
                {
                    var names = ExoData.GetNames(scanOrganic.Variant);

                    if (names is not null)
                    {
                        item.AddVariant(scanOrganic.Variant, names.Variant, currentRegion, state);
                    }
                    else
                    {
                        item.AddVariant(scanOrganic.Variant, scanOrganic.Variant_Localised, currentRegion, state);
                    }
                }

                if (parserStore.IsLive)
                    OnSpeciesUpdated?.Invoke(this, scanOrganic.Genus);

                return;
            }

            var other = OrganicScanItems["Other"];

            var known = other.FirstOrDefault(x => x.SpeciesCodex == scanOrganic.Species);

            if (known is null)
            {
                known = new(scanOrganic.Species, scanOrganic.Species_Localised);
                OrganicScanItems["Other"].Add(known);
                OrganicScanItems["Other"].Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
            }

            known.AddRegion(currentRegion, state);
        }

        private void MarkBioSold(BioData data)
        {
            if (OrganicScanItems.TryGetValue(data.Genus, out var items))
            {
                var item = items.FirstOrDefault(x => x.SpeciesCodex == data.Species);

                if (item is null)
                    return;

                var analysedItem = item.Region.FirstOrDefault(x => x.Value == OrganicScanState.Analysed);

                if (analysedItem.Value == OrganicScanState.Analysed)
                {
                    item.Region[analysedItem.Key] = OrganicScanState.Sold;
                }

                foreach (var variants in item.Variants)
                {
                    foreach (var variant in variants.Value)
                    {
                        if (variant.VaritantCodex == data.Variant && variant.State == OrganicScanState.Analysed)
                        {
                            variant.State = OrganicScanState.Sold;
                            if (parserStore.IsLive)
                                OnSpeciesUpdated?.Invoke(this, data.Genus);
                            return;
                        }
                    }
                }

                if (parserStore.IsLive)
                    OnSpeciesUpdated?.Invoke(this, data.Genus);
            }
        }

        private void MarkUnsoldLost()
        {
            foreach (var items in OrganicScanItems.Values)
            {
                if (items == null) continue;
                foreach (var item in items)
                {
                    foreach (var region in item.Region)
                    {
                        if (region.Value == OrganicScanState.Analysed)
                            item.Region[region.Key] = OrganicScanState.Discovered;
                    }

                    foreach (var variant in item.Variants)
                    {
                        foreach (var vari in variant.Value)
                        {
                            if (vari.State == OrganicScanState.Analysed)
                                vari.State = OrganicScanState.Discovered;
                        }
                    }
                }
            }

            if (parserStore.IsLive)
                OnOrganicScanDetailsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void AddCodex(string codexValue)
        {
            if (codexEntries.TryGetValue(currentRegion, out List<string>? value))
            {
                var known = value.FirstOrDefault(x => x == codexValue);

                if (known == null)
                {
                    codexEntries[currentRegion].Add(codexValue);
                }
                return;
            }

            codexEntries.TryAdd(currentRegion, [codexValue]);
        }

        public bool IsNewCodex(string codexValue)
        {
            if (codexEntries.TryGetValue(currentRegion, out List<string>? value))
            {
                var known = value.FirstOrDefault(x => x == codexValue);

                if (known == null)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private void AddSpecies(string species)
        {
            if (speciesEntries.TryGetValue(currentRegion, out List<string>? value))
            {
                var known = value.FirstOrDefault(x => x == species);

                if (known == null)
                {
                    speciesEntries[currentRegion].Add(species);
                }
                return;
            }

            speciesEntries.TryAdd(currentRegion, [species]);
        }

        public bool IsNewSpecies(string codexValue)
        {
            if (speciesEntries.TryGetValue(currentRegion, out List<string>? value))
            {
                var known = value.FirstOrDefault(x => x == codexValue);

                if (known == null)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public void ParseJournalEvent(JournalEntry e)
        {
            try
            {
                switch (e.EventData)
                {
                    case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                        {
                            currentRegion = (GalacticRegions)RegionMap.FindRegion(fsdJump.StarPos.X, fsdJump.StarPos.Y, fsdJump.StarPos.Z).Id;
                        }
                        break;
                    case CodexEntryEvent.CodexEntryEventArgs codex:
                        {
                            currentRegion = codex.Region;

                            if (codex.Category != "$Codex_Category_Biology;")
                                break;
                            
                            if (string.IsNullOrEmpty(codex.Name))
                                break;

                            AddCodex(codex.Name);                            

                            var bioNames = ExoData.GetNames(codex.Name);

                            if (bioNames == null)
                                break;
                                                   
                            if (OrganicScanItems.TryGetValue(bioNames.GenusCodex, out var items))
                            {
                                var item = items.FirstOrDefault(x => x.SpeciesCodex == bioNames.SpeciesCodex);

                                if (item is null)
                                    return;

                                item.AddRegion(currentRegion, OrganicScanState.Discovered);

                                if (item.Variants.TryGetValue(currentRegion, out var variants))
                                {
                                    var knownVariant = variants.FirstOrDefault(x => x.VaritantCodex == codex.Name);

                                    if(knownVariant == null)
                                    {
                                        knownVariant = new(codex.Name, codex.Name_Localised, currentRegion, OrganicScanState.Discovered);
                                        variants.Add(knownVariant);
                                    }

                                    knownVariant.State = OrganicScanState.Discovered;

                                    if (parserStore.IsLive)
                                        OnSpeciesUpdated?.Invoke(this, bioNames.GenusCodex);
                                }
                            }
                        }
                        break;
                    case ScanOrganicEvent.ScanOrganicEventArgs scanOrganic:
                        {
                            try
                            {
                                AddBioScan(scanOrganic);
                                if (string.IsNullOrEmpty(scanOrganic.Species) == false)
                                    AddSpecies(scanOrganic.Species);
                                if (string.IsNullOrEmpty(scanOrganic.Variant) == false)
                                    AddCodex(scanOrganic.Variant);
                            }
                            catch (NullReferenceException nullRef)
                            {
                                App.Logger.Error(nullRef.Message);
                            }
                            catch (Exception ex)
                            {
                                App.Logger.Error(ex.Message);
                            }
                        }
                        break;
                    case SellOrganicDataEvent.SellOrganicDataEventArgs sellOrganic:
                        {
                            foreach (var organic in sellOrganic.BioData)
                            {
                                MarkBioSold(organic);
                            }
                        }
                        break;
                    case DiedEvent.DiedEventArgs:
                        {
                            MarkUnsoldLost();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Error(ex, "Exception parsing journal logs");
            }
        }

        private void ParserStore_OnParserStoreLive(object? sender, bool e)
        {
            OnOrganicScanDetailsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void ClearData()
        {
            OrganicScanItems.Clear();
            speciesEntries.Clear();
            codexEntries.Clear();
            BuildDictionary();
        }

        public void Dispose()
        {
            parserStore.UnregisterParser(this);
        }

        public JournalHistoryArgs GetEventsToParse(DateTime defaultAge)
        {
            return new(historicEventsToParse, historicAge, this, ParseHistoryStream);
        }

        public void RunBeforeParsingLogs(int currentCmdrId)
        {

        }

        public Task ParseHistoryStream(JournalEntry entry)
        {
            ParseJournalEvent(entry);
            return Task.CompletedTask;
        }

        public void ParseHistory(IEnumerable<JournalEntry> journalEntries, int commanderId)
        {
            foreach (var entry in journalEntries)
            {
                ParseJournalEvent(entry);
            }
        }
    }
}
