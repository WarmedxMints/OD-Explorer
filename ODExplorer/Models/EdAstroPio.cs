using ODExplorer.Database.DTOs;
using ODUtils.Models;
using ODUtils.Models.EdAstro;
using System;

namespace ODExplorer.Models
{
    public sealed class EdAstroPoi(EdAstroPoiDTO dTO)
    {
        public int Id { get; set; } = dTO.Id;
        public string Name { get; set; } = dTO.Name;
        public string GalMapName { get; set; } = dTO.GalMapName;
        public long SystemAddress { get; set; } = dTO.SystemAddress;
        public Position SystemPosition { get; set; } = new(dTO.X, dTO.Y, dTO.Z);
        public EDAstroType Type { get; set; } = (EDAstroType)dTO.Type;
        public EDAstroType Type2 { get; set; } = (EDAstroType)dTO.Type2;
        public string Summary { get; set; } = dTO.Summary;
        public string MarkDown { get; set; } = dTO.MarkDown;
        public double DistanceFromSol { get; set; } = dTO.DistanceFromSol;
        public Uri PoiUrl { get; set; } = new(dTO.PoiUrl);
    }
}
