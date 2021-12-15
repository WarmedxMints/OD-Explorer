using ODExplorer.Utils;

namespace ODExplorer.OrganicData
{
    public class BioData : PropertyChangeNotify
    {
        private string _species;
        public string Species { get => _species; set { _species = value; OnPropertyChanged(); } }

        private string _status;

        public string Status
        {
            get => _status;
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
                _status = value;
                OnPropertyChanged();
            }
        }

        private string _variant;
        public string Variant { get => _variant; set { _variant = value; OnPropertyChanged(); } }
    }
}
