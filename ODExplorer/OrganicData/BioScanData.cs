using Newtonsoft.Json;
using ODExplorer.Utils;

namespace ODExplorer.OrganicData
{
    public sealed class BioScanData : PropertyChangeNotify
    {
        private double longtitude;
        private double latitude;
        private string scanType;
        private string distanceToScan = "N/A";
        private bool farEnoughFromScan = false;
        private bool notified = true;
        public double Longtitude { get => longtitude; set { longtitude = value; OnPropertyChanged(); } }
        public double Latitude { get => latitude; set { latitude = value; OnPropertyChanged(); } }
        public string ScanType { get { return scanType; } set { scanType = value; OnPropertyChanged(); } }
        public string DistanceToScan { get { return distanceToScan; } set { distanceToScan = value; OnPropertyChanged(); } }
        [JsonIgnore]
        public bool FarEnoughFromScan { get { return farEnoughFromScan; } set { farEnoughFromScan = value; OnPropertyChanged(); } }
        [JsonIgnore]
        public bool Notified { get { return notified; } set { notified = value; OnPropertyChanged(); } }
    }
}
