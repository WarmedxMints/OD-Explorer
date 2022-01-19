using ODExplorer.Utils;

namespace ODExplorer.OrganicData
{
    public class BiologicalInfo : PropertyChangeNotify
    {
        private int value;
        public int Value { get => value; set { this.value = value; OnPropertyChanged(); } }

        private int colonyRange;
        public int ColonyRange { get => colonyRange; set { colonyRange = value; OnPropertyChanged(); } }
    }
}
