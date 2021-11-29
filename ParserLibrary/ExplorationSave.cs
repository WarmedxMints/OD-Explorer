using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserLibrary
{
    [Serializable]
    public class ExplorationSave
    {
        public List<ExplorationTarget> Systems { get; set; }
        public int CurrentIndex { get; set; }

        public ExplorationSave() { }
        public ExplorationSave(ExplorationTargets explorationTargets)
        {
            Systems = explorationTargets.Targets.ToList();
            CurrentIndex = explorationTargets.CurrentIndex;
        }
    }
}