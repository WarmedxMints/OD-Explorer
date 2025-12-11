using ODExplorer.Models;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Models;
using ODUtils.Models.EdAstro;
using System;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class EdAstroPoiViewModel : OdViewModelBase
    {
        private readonly EdAstroPoi poi;

        public EdAstroPoiViewModel(EdAstroPoi poi, Position currentPosition)
        {
            this.poi = poi;
            UpdateDistance(currentPosition);
        }

        public int Id => poi.Id;
        public string Name => poi.Name;
        public string GalMapName => poi.GalMapName;
        public long Address => poi.SystemAddress;
        public Position Position => poi.SystemPosition;
        public EDAstroType Type1 => poi.Type;
        public EDAstroType Type2 => poi.Type;
        public string Summary => poi.Summary;
        public Uri PioUrl => poi.PoiUrl;
        public string MarkDown => poi.MarkDown.Contains("(/poiphotos") ? 
            poi.MarkDown.Replace("(/poiphotos", "(https://edastro.com/poiphotos") 
            : poi.MarkDown;
        public double DistanceFromCommander { get; private set; } 

        internal void UpdateDistance(Position position)
        {
            DistanceFromCommander = Position.DistanceFrom(position);
            OnPropertyChanged(nameof(DistanceFromCommander));
        }
    }
}
