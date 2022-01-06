using ODExplorer.Utils;
using System.Collections.ObjectModel;

namespace ODExplorer.OrganicData
{
    public class BiologicalData : PropertyChangeNotify
    {
        private string systemName = "Unknown";
        public string SystemName { get => systemName; set { systemName = value; OnPropertyChanged(); } }

        private string bodyName = "-";
        public string BodyName { get => bodyName; set { bodyName = value; OnPropertyChanged(); } }

        private string bodyType = "?";
        public string BodyType { get => bodyType; set { bodyType = value; OnPropertyChanged(); } }

        private string atmosphereType = "?";
        public string AtmosphereType { get => atmosphereType; set { atmosphereType = value; OnPropertyChanged(); } }

        private string volcanism = "?";
        public string Volcanism { get => volcanism; set { volcanism = value; OnPropertyChanged(); } }

        private double surfacePressure;
        public double SurfacePressure { get => surfacePressure; set { surfacePressure = value; OnPropertyChanged(); } }

        private double surfaceGravity;
        public double SurfaceGravity { get => surfaceGravity; set { surfaceGravity = value; OnPropertyChanged(); } }

        private int surfaceTemp;
        public int SurfaceTemp { get => surfaceTemp; set { surfaceTemp = value; OnPropertyChanged(); } }

        public ObservableCollection<BioData> BodyBioData { get; set; } = new();

        public override string ToString()
        {
            return bodyName;
        }

        public int GetBodyValue()
        {
            if(BodyBioData.Count == 0)
            {
                return 0;
            }

            int ret = 0;

            foreach(BioData body in BodyBioData)
            {
                ret += body.GetValueToCommander();
            }

            return ret;
        }
    }
}