using Newtonsoft.Json;
using ODUtils.Spansh;
using System.Collections.Generic;
using System.Linq;

namespace ODExplorer.Models
{
    public sealed class SpanshCsvContainer
    {
        public SpanshCsvContainer(List<ExplorationTarget> targets, int currentIndex)
        {
            Targets = targets;
            CurrentIndex = currentIndex;
            CurrentTarget = targets.FirstOrDefault();
        }

        public List<ExplorationTarget> Targets { get; set; }
        [JsonIgnore]
        public ExplorationTarget? CurrentTarget { get; set; }
        public int CurrentIndex { get; set; }
        [JsonIgnore]
        public bool HasValue => Targets.Count > 0;
    }
}
