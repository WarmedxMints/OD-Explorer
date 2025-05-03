using EliteJournalReader.Events;
using ODUtils.Dialogs.ViewModels;
using ODUtils.EliteDangerousHelpers;

namespace ODExplorer.ViewModels.ModelVMs
{
    /*   public struct PlanetRing
    {
        public string Name { get; set; }
        public string RingClass { get; set; }
        public double MassMT { get; set; }
        public double InnerRad { get; set; }
        public double OuterRad { get; set; }
    }*/
    public sealed class PlanetRingViewModel(PlanetRing ring, string ownerName) : OdViewModelBase
    {
        public string Name => ring.Name.Replace(ownerName, "").Trim();
        public string RingClass => GetRingClass(ring.RingClass);
        public string MassMT => ring.MassMT.ToString("N0");
        public string InnRad => BodyHelpers.FormatMeters(ring.InnerRad);
        public string OutRad => BodyHelpers.FormatMeters(ring.OuterRad);
        private static string GetRingClass(string ringClass)
        {
            switch (ringClass)
            {
                case "eRingClass_Icy":
                    return "Icy";
                case "eRingClass_Metalic":
                    return "Metallic";
                case "eRingClass_MetalRich":
                    return "Metal Rich";
                case "eRingClass_Rocky":
                    return "Rock";
            }
            return "Unknown";
        }
    }
}
