using ODExplorer.Utils;

namespace ODExplorer.OrganicData
{
    public class BiologicalInfo : PropertyChangeNotify
    {
        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private int value;
        public int Value { get => value; set { this.value = value; OnPropertyChanged(); } }

        private int colonyRange;
        public int ColonyRange { get => colonyRange; set { colonyRange = value; OnPropertyChanged(); } }
    }
}
