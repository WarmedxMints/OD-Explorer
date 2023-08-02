using EliteJournalReader;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ODExplorer.Utils.Converters
{
    public class AstmosphereClassToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not AtmosphereClass)
            {
                return null;
            }

            AtmosphereClass atmosphereType = (AtmosphereClass)value;

            string ret = atmosphereType switch
            {
                AtmosphereClass.Unknown => "?",
                AtmosphereClass.None => "None",
                AtmosphereClass.NoAtmosphere => "No Atm",
                AtmosphereClass.SuitableForWaterBasedLife => "SWBL",
                AtmosphereClass.AmmoniaOxygen => "NH\u2083 O\u2082",
                AtmosphereClass.Ammonia => "NH\u2083",
                AtmosphereClass.Water => "H\u2082O",
                AtmosphereClass.CarbonDioxide => "CO\u2082",
                AtmosphereClass.SulphurDioxide => "SO\u2082",
                AtmosphereClass.Nitrogen => "N\u2082",
                AtmosphereClass.WaterRich => "H\u2082O Rich",
                AtmosphereClass.MethaneRich => "CH\u2084 Rich",
                AtmosphereClass.AmmoniaRich => "NH\u2083 Rich",
                AtmosphereClass.CarbonDioxideRich => "CO\u2082 Rich",
                AtmosphereClass.Methane => "CH\u2084",
                AtmosphereClass.Helium => "He",
                AtmosphereClass.SilicateVapour => "Sil Vap",
                AtmosphereClass.MetallicVapour => "Met Vap",
                AtmosphereClass.NeonRich => "Ne Rich",
                AtmosphereClass.ArgonRich => "Ar Rich",
                AtmosphereClass.Neon => "Ne",
                AtmosphereClass.Argon => "Ar",
                AtmosphereClass.Oxygen => "O₂",
                AtmosphereClass.EarthLike => "N₂ O₂",
                _ => string.Empty,
            };
            return ret;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}