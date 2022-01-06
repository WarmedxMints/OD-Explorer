using System;
using System.ComponentModel;

namespace ODExplorer.AppSettings.NoteableBody
{
    [Flags]
    public enum NoteablePlanetClass
    {
        [Description("Select All")]
        Any = -1,
        [Description("Unkown")]
        Unknown = 1 << 0,
        [Description("Ammonia World")]
        AmmoniaWorld = 1 << 1,
        [Description("Earthlike Body")]
        EarthlikeBody = 1 << 2,
        [Description("Gas Giant With Ammonia Based Life")]
        GasGiantWithAmmoniaBasedLife = 1 << 3,
        [Description("Gas Giant With Water Based Life")]
        GasGiantWithWaterBasedLife = 1 << 4,
        [Description("Helium Gas Giant")]
        HeliumGasGiant = 1 << 5,
        [Description("Helium Rich Gas Giant")]
        HeliumRichGasGiant = 1 << 6,
        [Description("High Metal Content Body")]
        HighMetalContentBody = 1 << 7,
        [Description("Icy Body")]
        IcyBody = 1 << 8,
        [Description("Metal Rich Body")]
        MetalRichBody = 1 << 9,
        [Description("Rocky Body")]
        RockyBody = 1 << 10,
        [Description("Rocky Ice Body")]
        RockyIceBody = 1 << 11,
        [Description("Sudarsky Class I Gas Giant")]
        SudarskyClassIGasGiant = 1 << 12,
        [Description("Sudarsky Class Ii Gas Giant")]
        SudarskyClassIIGasGiant = 1 << 13,
        [Description("Sudarsky Class Iii Gas Giant")]
        SudarskyClassIIIGasGiant = 1 << 14,
        [Description("Sudarsky Class Iv Gas Giant")]
        SudarskyClassIVGasGiant = 1 << 15,
        [Description("Sudarsky Class V Gas Giant")]
        SudarskyClassVGasGiant = 1 << 16,
        [Description("Water Giant")]
        WaterGiant = 1 << 17,
        [Description("Water Giant With Life")]
        WaterGiantWithLife = 1 << 18,
        [Description("Water World")]
        WaterWorld = 1 << 19,
    }

    public enum LandableStatus
    {
        [Description("Any")]
        Any,
        [Description("Landable")]
        Landable,
        [Description("Not Landable")]
        NotLandable
    }

    public enum TerraformableStatus
    {
        [Description("Any")]
        Any,
        [Description("Terraformable")]
        Terraformable,
        [Description("Not Terraformable")]
        NotTerraformable
    }
}
