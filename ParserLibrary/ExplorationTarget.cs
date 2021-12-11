using System;
using System.Collections.Generic;

namespace ParserLibrary
{
    [Serializable]
    public class ExplorationTarget
    {
        //"System Name","Distance","Distance Remaining","Tritium in tank","Tritium in market","Fuel Used","Icy Ring","Pristine","Restock Tritium"
        public string SystemName { get; set; } = "No Data";
        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }

        public List<BodiesInfo> BodiesInfo { get; set; }
    }
}