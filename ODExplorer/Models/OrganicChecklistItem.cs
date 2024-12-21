using EliteJournalReader;
using ODUtils.Models;
using System.Collections.Generic;
using System.Linq;

namespace ODExplorer.Models
{
    public sealed class OrganicChecklistItem(string codexValue, string englishName)
    {
        public string SpeciesCodex { get; } = codexValue;
        public string Name { get; } = englishName;
        public Dictionary<GalacticRegions, OrganicScanState> Region { get; } = [];
        public Dictionary<GalacticRegions, List<OrganicChechListItemVariant>> Variants { get; } = [];

        public void AddRegion(GalacticRegions region, OrganicScanState state)
        {
            if (Region.TryGetValue(region, out OrganicScanState value))
            {
                if (value > state)
                {
                    return;
                }

                Region[region] = state;
                return;
            }

            Region.Add(region, state);
        }

        public void AddVariant(string codexValue, string localName, GalacticRegions region, OrganicScanState state)
        {
            if (Variants.TryGetValue(region, out List<OrganicChechListItemVariant>? value))
            {
                value ??= [];

                var known = value.FirstOrDefault(x => x.VaritantCodex == codexValue);

                if (known is null)
                {
                    known = new(codexValue, localName, region, state);
                    value.Add(known);
                    value.Sort((x, y) => string.Compare(x.LocalName, y.LocalName, System.StringComparison.Ordinal));
                    return;
                }

                if (known.State < state)
                    known.State = state;
                return;
            }

            var newVariant = new OrganicChechListItemVariant(codexValue, localName, region, state);
            Variants.Add(region, [newVariant]);
        }
    }
}
