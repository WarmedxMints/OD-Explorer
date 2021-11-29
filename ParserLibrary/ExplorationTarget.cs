using System;
using System.Collections.Generic;

namespace ParserLibrary
{
    [Serializable]
    public class ExplorationTarget
    {
        public string SystemName { get; set; }

        public List<BodiesInfo> BodiesInfo { get; set; }
    }
}