namespace ODExplorer.Database.DTOs
{
    public sealed class EdAstroPoiDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GalMapName { get; set; } = string.Empty;
        public long SystemAddress { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int Type { get; set; }
        public int Type2 { get; set; }
        public string Summary { get; set; } = string.Empty;
        public double DistanceFromSol { get; set; }
        public string PoiUrl { get; set; } = string.Empty;
        public string MarkDown { get; set; } = string.Empty;
    }
}
