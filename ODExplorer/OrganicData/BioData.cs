using ODExplorer.Utils;

namespace ODExplorer.OrganicData
{
    public class BioData : PropertyChangeNotify
    {
        private string timeStamp;
        public string TimeStamp { get => timeStamp; set { timeStamp = value; OnPropertyChanged(); } }

        private string species;
        public string Species { get => species; set { species = value; OnPropertyChanged(); } }

        private string status;

        public string Status
        {
            get => status;
            set
            {
                switch (value)
                {
                    case "Log":
                        value = "LOGGED";
                        break;
                    case "Sample":
                        value = "SAMPLED";
                        break;
                    case "Analyse":
                        value = "ANALYSED";
                        break;
                    default:
                        break;
                }
                status = value;
                OnPropertyChanged();
            }
        }

        private string variant;
        public string Variant { get => variant; set { variant = value; OnPropertyChanged(); } }

        private BiologicalInfo bioInfo;
        public BiologicalInfo BioInfo { get => bioInfo; set { bioInfo = value; OnPropertyChanged(); } }

        public int GetValueToCommander()
        {
            return status == "ANALYSED" ? BioInfo.Value : 0;
        }
    }
}
