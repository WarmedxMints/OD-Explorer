using EliteJournalReader;
using ODUtils.Models;

namespace ODExplorer.Models
{
    public sealed class OrganicChechListItemVariant(string variantCodex, string localName, GalacticRegions region, OrganicScanState state)
    {
        public string VaritantCodex { get; } = variantCodex;
        public string LocalName { get; } = localName;
        public GalacticRegions Region { get; } = region;
        public OrganicScanState State { get; set; } = state;
    }
}
