using ODExplorer.Utils;

namespace ODExplorer.Preset
{
    public class ODPresetMenuItem : PropertyChangeNotify
    {
        private string header;
        public string Header { get => header; set { header = value; OnPropertyChanged(); ; } }
    }
}
