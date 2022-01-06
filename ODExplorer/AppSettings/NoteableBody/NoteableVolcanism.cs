using System;
using System.Runtime.Serialization;

namespace ODExplorer.AppSettings.NoteableBody
{
    public class NoteableVolcanism : MenuFromMenuInfoArrayBase
    {
        [IgnoreDataMember]
        public override string[] MenuInfoArray { get; } = {
                "Ammonia Magma",
                "Carbon Dioxide Geysers",
                "Major Carbon Dioxide Geysers",
                "Major Metallic Magma","Major Rocky Magma",
                "Major Silicate Vapour Geysers",
                "Major Water Geysers",
                "Major Water Magma",
                "Metallic Magma",
                "Methane Magma",
                "Minor Ammonia Magma",
                "Minor Carbon Dioxide Geysers",
                "Minor Metallic Magma",
                "Minor Methane Magma",
                "Minor Nitrogen Magma",
                "Minor Rocky Magma",
                "Minor Silicate Vapour Geysers",
                "Minor Water Geysers",
                "Minor Water Magma",
                "Nitrogen Magma",
                "No Volcanism",
                "Rocky Magma",
                "Silicate Vapour Geysers",
                "Water Geysers",
                "Water Magma"
        };
        [IgnoreDataMember]
        protected override string InfoNullReturn => "No Volcanism";

        protected override string StringInfoOperations(string infoString)
        {
            if (infoString.Contains("volcanism", StringComparison.OrdinalIgnoreCase))
            {
                infoString = infoString.Remove(infoString.Length - 10);
            }

            return infoString;
        }
    }
}

