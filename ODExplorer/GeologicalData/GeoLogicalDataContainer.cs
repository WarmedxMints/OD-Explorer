using ODExplorer.Utils;
using System.Collections.ObjectModel;

namespace ODExplorer.GeologicalData
{
    public class GeoLogicalDataContainer : PropertyChangeNotify
    {
        private string _systemName;
        public string SystemName { get => _systemName; set { _systemName = value; OnPropertyChanged(); } }

        private string _bodyName;
        public string BodyName { get => _bodyName; set { _bodyName = value; OnPropertyChanged(); } }

        public ObservableCollection<GeoData> BodyBioData { get; set; } = new();

        public override string ToString()
        {
            return _bodyName;
        }
    }
}