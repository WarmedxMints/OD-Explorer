using ODExplorer.Utils;
using ParserLibrary;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ODExplorer.CsvControl
{
    public class CsvContainer : PropertyChangeNotify
    {
        private List<ExplorationTarget> targets = new();
        public List<ExplorationTarget> Targets { get => targets; set { targets = value; OnPropertyChanged(); } }

        private ExplorationTarget currentTarget;
        [IgnoreDataMember]
        public ExplorationTarget CurrentTarget { get => currentTarget; set { currentTarget = value; OnPropertyChanged(); } }
        private int currentIndex;
        public int CurrentIndex { get => currentIndex; set { currentIndex = value; OnPropertyChanged(); } }
    }
}
