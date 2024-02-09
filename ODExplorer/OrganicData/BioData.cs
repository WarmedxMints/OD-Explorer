using ODExplorer.Utils;
using System.Collections.ObjectModel;

namespace ODExplorer.OrganicData
{
    public class BioData : PropertyChangeNotify
    {
        private string timeStamp;
        public string TimeStamp { get => timeStamp; set { timeStamp = value; OnPropertyChanged(); } }

        private string species;
        public string Species { get => species; set { species = value; OnPropertyChanged(); } }

        private string status;

        private ObservableCollection<BioScanData> scanData = new();

        public ObservableCollection<BioScanData> ScanData
        {
            get { return scanData; }
            set { scanData = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => status;
            set
            {
                switch (value)
                {
                    case "Dss":
                        value = "REPORTED";
                        break;
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

        private string codexVariant = string.Empty;
        public string CodexVariant { get => codexVariant; set { codexVariant = value; OnPropertyChanged(); } }

        private bool sold;
        public bool Sold { get => sold; set => sold = value; }

        private BiologicalInfo bioInfo;
        public BiologicalInfo BioInfo { get => bioInfo; set { bioInfo = value; OnPropertyChanged(); } }

        public int GetValueToCommander()
        {
            return status == "ANALYSED" ? BioInfo.Value : 0;
        }
    }
}
