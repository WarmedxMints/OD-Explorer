using EliteJournalReader;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ODExplorer.OrganicData
{
    internal enum GalacticRegions
    {
        Any,
        ScutumCentaurus,
    }
    internal sealed class BioProperties
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public AtmosphereClass AtmosphereClass { get; set; }
        public double? MinGravity { get; set; }
        public double? MaxGravity { get; set; }
        public double? MinTemp { get; set; }
        public double? MaxTemp { get; set; }
        public PlanetClass PlanetClass { get; set; }
        public string Volcanism { get; set; }
        public GalacticRegions Region { get; set; } = GalacticRegions.Any;
        public StarType StarType { get; set; }
        public bool Nebula { get; set; }
        public string[] Regions { get; set; }
    }
    internal sealed class BioDataValues
    {
        
        internal static Dictionary<string, Dictionary<string, BioProperties>> BiologicalDatabase = new()
        {
            #region Aleoida
            { "$Codex_Ent_Aleoids_Genus_Name", new Dictionary<string, BioProperties>()
            {
                { "$Codex_Ent_Aleoids_01_Name",
                    new BioProperties()
                    {
                        Name = "Aleoida Arcus",
                        Value = 7252500,
                        AtmosphereClass = AtmosphereClass.CarbonDioxide | AtmosphereClass.CarbonDioxideRich,
                        MaxGravity = 0.275,
                        MaxTemp = 180,
                        MinTemp = 175,
                        PlanetClass = PlanetClass.RockyBody | PlanetClass.HighMetalContentBody
                    }
                },
                { "$Codex_Ent_Aleoids_02_Name",
                    new BioProperties()
                    {
                        Name = "Aleoida Coronamus",
                        Value = 6284600,
                        AtmosphereClass = AtmosphereClass.CarbonDioxide | AtmosphereClass.CarbonDioxideRich,
                        MaxGravity = 0.275,
                        MaxTemp = 190,
                        MinTemp = 180,
                        PlanetClass = PlanetClass.RockyBody | PlanetClass.HighMetalContentBody
                    }
                },
                { "$Codex_Ent_Aleoids_03_Name",
                    new BioProperties()
                    {
                        Name = "Aleoida Spica",
                        Value = 3385200,
                        AtmosphereClass = AtmosphereClass.Ammonia,
                        MaxGravity = 0.275,
                        MaxTemp = 177,
                        MinTemp = 170,
                        PlanetClass = PlanetClass.RockyBody | PlanetClass.HighMetalContentBody,
                        Region = GalacticRegions.ScutumCentaurus
                    }
                },
                { "$Codex_Ent_Aleoids_04_Name",
                    new BioProperties()
                    {
                        Name = "Aleoida Laminiae",
                        Value = 3385200,
                        AtmosphereClass = AtmosphereClass.Ammonia,
                        MaxGravity = 0.275,
                        MaxTemp = 177,
                        MinTemp = 152,
                        PlanetClass = PlanetClass.RockyBody | PlanetClass.HighMetalContentBody,
                        Region = GalacticRegions.ScutumCentaurus
                    }
                },
                { "$Codex_Ent_Aleoids_05_Name",
                    new BioProperties()
                    {
                        Name = "Aleoida Gravis",
                        Value = 12934900,
                        AtmosphereClass = AtmosphereClass.CarbonDioxide | AtmosphereClass.CarbonDioxideRich,
                        MaxGravity = 0.275,
                        MaxTemp = 196,
                        MinTemp = 190,
                        PlanetClass = PlanetClass.RockyBody | PlanetClass.HighMetalContentBody
                    }
                }
            }
            },
            #endregion
            #region Bacterium
            { "$Codex_Ent_Bacterial_Genus_Name", new Dictionary<string, BioProperties>()
            {
                { "$Codex_Ent_Bacterial_01_Name",
                    new BioProperties()
                    {
                        Name = "Bacterium Aurasus",
                        Value = 1000000,
                        AtmosphereClass = AtmosphereClass.CarbonDioxide | AtmosphereClass.CarbonDioxideRich,
                        MinGravity = 0.039,
                        MaxGravity = 0.605,
                        MaxTemp = 400,
                        MinTemp = 145,
                        PlanetClass = PlanetClass.RockyBody | PlanetClass.HighMetalContentBody
                    }
                },
                { "$Codex_Ent_Bacterial_02_Name",
                    new BioProperties()
                    {
                        Name = "Bacterium Nebulus",
                        Value = 5289900,
                        AtmosphereClass = AtmosphereClass.Helium,
                        MinGravity = 0.4,
                        MaxGravity = 0.55,
                        MaxTemp = 21,
                        MinTemp = 20,
                        PlanetClass = PlanetClass.IcyBody
                    }
                },
                { "$Codex_Ent_Bacterial_03_Name",
                    new BioProperties()
                    {
                        Name = "Bacterium Scopulum",
                        Value = 4934500,
                        AtmosphereClass = AtmosphereClass.Argon | AtmosphereClass.Neon | AtmosphereClass.NeonRich | AtmosphereClass.Methane | AtmosphereClass.Helium,
                        MinGravity = 0.025,
                        MaxGravity = 0.6,
                        PlanetClass = PlanetClass.IcyBody | PlanetClass.RockyIceBody
                    }
                },
            }
            #endregion
            }
        };   
    }
}
