using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ODExplorer.EDSM
{
    public class EDSMSystem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("id64")]
        public long Id64 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("estimatedValue")]
        public long EstimatedValue { get; set; }

        [JsonProperty("estimatedValueMapped")]
        public long EstimatedValueMapped { get; set; }

        [JsonProperty("valuableBodies")]
        public List<ValuableBody> ValuableBodies { get; set; }
    }

    public class ValuableBody
    {
        [JsonProperty("bodyId")]
        public long BodyId { get; set; }

        [JsonProperty("bodyName")]
        public string BodyName { get; set; }

        [JsonProperty("distance")]
        public long Distance { get; set; }

        [JsonProperty("valueMax")]
        public long ValueMax { get; set; }
    }
}
