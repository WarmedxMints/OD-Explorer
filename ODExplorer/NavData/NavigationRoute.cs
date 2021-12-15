using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using EliteJournalReader;

namespace ODExplorer.NavData
{
    //Class used to Deserialize the NavRoute.json file ED produces upon plotting a route in game
    public class NavigationRoute
    {
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("event", NullValueHandling = NullValueHandling.Ignore)]
        public string Event { get; set; }

        [JsonProperty("Route", NullValueHandling = NullValueHandling.Ignore)]
        public List<Route> Route { get; set; }

        public static NavigationRoute FromJson(string json) => JsonConvert.DeserializeObject<NavigationRoute>(json, Converter.Settings);
    }

    public class Route
    {
        [JsonProperty("StarSystem", NullValueHandling = NullValueHandling.Ignore)]
        public string StarSystem { get; set; }

        [JsonProperty("SystemAddress", NullValueHandling = NullValueHandling.Ignore)]
        public long SystemAddress { get; set; }

        [JsonProperty("StarPos", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(SystemPositionConverter))]
        public SystemPosition StarPos { get; set; }

        [JsonProperty("StarClass", NullValueHandling = NullValueHandling.Ignore)]
        public string StarClass { get; set; }
    }

    public static class Serialize
    {
        public static string ToJson(this NavigationRoute self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
