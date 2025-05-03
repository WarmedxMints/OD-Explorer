using ODExplorer.Stores;
using ODExplorer.ViewModels.ModelVMs;
using ODUtils.Dialogs.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class EdAstroViewModel : OdViewModelBase
    {
        private readonly ExplorationDataStore explorationDataStore;

        public EdAstroViewModel(ExplorationDataStore explorationDataStore)
        {
            this.explorationDataStore = explorationDataStore;
            this.explorationDataStore.OnCurrentSystemUpdated += ExplorationDataStore_OnCurrentSystemUpdated;
            PopulatePois();
        }

        public override void Dispose()
        {
            this.explorationDataStore.OnCurrentSystemUpdated -= ExplorationDataStore_OnCurrentSystemUpdated;
        }

        private List<EdAstroPoiViewModel> pointsOfInterest = [];
        public List<EdAstroPoiViewModel> PointsOfInterest
        {
            get => pointsOfInterest;
            set
            {
                pointsOfInterest = value;
                OnPropertyChanged(nameof(PointsOfInterest));    
            }
        }

        private EdAstroPoiViewModel? selectedPoi;
        public EdAstroPoiViewModel? SelectedPoi
        {
            get => selectedPoi;
            set
            {
                selectedPoi = value;
                OnPropertyChanged(nameof(SelectedPoi));
            }
        }

        private void PopulatePois()
        {
            if (explorationDataStore.EdAstroPois is null || explorationDataStore.EdAstroPois.Count == 0)
                return;


            var currentPosition = explorationDataStore.CurrentSystem?.Position ?? new(0, 0, 0);
            var pois = explorationDataStore.EdAstroPois.Select(x => new EdAstroPoiViewModel(x, currentPosition)).ToList();            

            pois.Sort((x, y) => x.DistanceFromCommander.CompareTo(y.DistanceFromCommander));

            pointsOfInterest.Clear();
            pointsOfInterest.AddRange(pois);
            SelectedPoi = pointsOfInterest.FirstOrDefault();
            OnPropertyChanged(nameof(PointsOfInterest));
        }

        private void ExplorationDataStore_OnCurrentSystemUpdated(object? sender, ODUtils.Models.StarSystem? e)
        {
            if (pointsOfInterest is null || pointsOfInterest.Count == 0 || e is null)
                return;

            foreach (var item in pointsOfInterest)
            {
                item.UpdateDistance(e.Position);
            }

            pointsOfInterest.Sort((x, y) => x.DistanceFromCommander.CompareTo(y.DistanceFromCommander));
            OnPropertyChanged(nameof(PointsOfInterest));
        }
    }
}
