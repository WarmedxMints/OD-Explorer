using ODUtils.Spansh;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ODExplorer.Models
{
    public sealed class SpanshCSVSettings
    {
        public Dictionary<int, CsvType> SelectedCSV { get; set; } = [];
        public bool AutoCopySystemToClipboard { get; set; } = true;
        public bool AutoStartFleetCarrierTimer { get; set; } = true;
        public string? CustomTimerSound { get; set; }
        public bool NotifyOnFcTimerCompletion { get; set; } = true;

        [IgnoreDataMember]
        public CsvType this[int index]
        {
            get
            {
                if (SelectedCSV.TryGetValue(index, out CsvType csvType))
                {
                    return csvType;
                }
                SelectedCSV.Add(index, CsvType.RoadToRiches);

                return CsvType.RoadToRiches;
            }
            set
            {
                if (SelectedCSV.ContainsKey(index))
                {
                    SelectedCSV[index] = value;
                    return;
                }

                SelectedCSV.TryAdd(index, value);
            }
        }
    }
}
