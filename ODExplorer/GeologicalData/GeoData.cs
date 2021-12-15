using ODExplorer.Utils;

namespace ODExplorer.GeologicalData
{
    public class GeoData : PropertyChangeNotify
    {
        private string _geoName;
        public string GeoName { get => _geoName; set { _geoName = value; OnPropertyChanged(); } }

        private int value;
        public int Value { get => value; set { this.value = value; OnPropertyChanged(); } }
    }
}