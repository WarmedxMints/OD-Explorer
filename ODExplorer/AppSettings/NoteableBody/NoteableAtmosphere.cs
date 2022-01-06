using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace ODExplorer.AppSettings.NoteableBody
{
    public class NoteableAtmosphere : MenuFromMenuInfoArrayBase
    {
        [IgnoreDataMember]
        public override string[] MenuInfoArray { get; } = {
            "Ammonia",
            "Ammonia and Oxygen",
            "Ammonia Rich",
            "Argon",
            "Argon Rich",
            "Carbon Dioxide",
            "Carbon Dioxide Rich",
            "Helium",
            "Hot Argon",
            "Hot Argon Rich",
            "Hot Carbon Dioxide",
            "Hot Carbon Dioxide Rich",
            "Hot Metallic Vapour",
            "Hot Silicate Vapour",
            "Hot Sulphur Dioxide",
            "Hot Water",
            "Hot Water Rich",
            "Hot thick Ammonia",
            "Hot thick Ammonia Rich",
            "Hot thick Argon",
            "Hot thick Argon Rich",
            "Hot thick Carbon Dioxide",
            "Hot thick Carbon Dioxide Rich",
            "Hot thick Metallic Vapour",
            "Hot thick Methane",
            "Hot thick Methane Rich",
            "Hot thick Nitrogen",
            "Hot thick No Atmosphere",
            "Hot thick Silicate Vapour",
            "Hot thick Sulphur Dioxide",
            "Hot thick Water",
            "Hot thick Water Rich",
            "Hot thin Carbon Dioxide",
            "Hot thin Metallic Vapour",
            "Hot thin Silicate Vapour",
            "Hot thin Sulphur Dioxide",
            "Metallic Vapour",
            "Methane",
            "Methane Rich",
            "Neon",
            "Neon Rich",
            "Nitrogen",
            "No Atmosphere",
            "Oxygen",
            "Silicate Vapour",
            "Suitable for Water Based Life",
            "Sulphur Dioxide",
            "Thick Ammonia",
            "Thick Ammonia and Oxygen",
            "Thick Ammonia Rich",
            "Thick Argon",
            "Thick Argon Rich",
            "Thick Carbon Dioxide",
            "Thick Carbon Dioxide Rich",
            "Thick Helium",
            "Thick Methane",
            "Thick Methane Rich",
            "Thick Nitrogen",
            "Thick No Atmosphere",
            "Thick Suitable for Water Based Life",
            "Thick Sulphur Dioxide",
            "Thick Water",
            "Thick Water Rich",
            "Thin Ammonia",
            "Thin Ammonia and Oxygen",
            "Thin Ammonia Rich",
            "Thin Argon",
            "Thin Argon Rich",
            "Thin Carbon Dioxide",
            "Thin Carbon Dioxide Rich",
            "Thin Helium",
            "Thin Methane",
            "Thin Methane Rich",
            "Thin Neon",
            "Thin Neon Rich",
            "Thin Nitrogen",
            "Thin No Atmosphere",
            "Thin Oxygen",
            "Thin Sulphur Dioxide",
            "Thin Water",
            "Thin Water Rich",
            "Water",
            "Water Rich",
        };
        [IgnoreDataMember]
        protected override string InfoNullReturn => "No Atmosphere";

        protected override string StringInfoOperations(string infoString)
        {
            if (infoString.Contains("atmosphere", StringComparison.OrdinalIgnoreCase))
            {
                infoString = infoString.Remove(infoString.Length - 11);
            }
            //Correct no British English spelling of Sulphur
            string pattern = @"\bsulfur\b";
            string replace = "sulphur";
            infoString = Regex.Replace(infoString, pattern, replace, RegexOptions.IgnoreCase);
            //I don't believe this is required as after pasing all of my own logs, a - was never used before 'rich'
            //However, I've noticed a - being used elsewhere so just in case, we check for it
            infoString = infoString.Replace("-", " ");

            return infoString;
        }
    }
}
