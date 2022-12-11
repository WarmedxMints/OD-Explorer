using EliteJournalReader.Events;
using LoadSaveSystem;
using ODExplorer.NavData;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace ODExplorer.OrganicData
{
    public class ScannedBioData : PropertyChangeNotify
    {
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "BioData.json");

        public ObservableCollection<BiologicalData> ScannedData { get; private set; } = new();

        private static readonly Dictionary<string, BiologicalInfo> bioValues = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "ALBIDUM SINUOUS TUBERS", new BiologicalInfo { Value = 111300, ColonyRange = 0   } },

            { "ALEOIDA ARCUS",          new BiologicalInfo { Value = 11670300, ColonyRange = 150 } },
            { "ALEOIDA CORONAMUS",      new BiologicalInfo { Value = 10432700, ColonyRange = 150 } },
            { "ALEOIDA GRAVIS",         new BiologicalInfo { Value = 16777215, ColonyRange = 150 } },
            { "ALEOIDA LAMINIAE",       new BiologicalInfo { Value = 6428600, ColonyRange = 150 } },
            { "ALEOIDA SPICA",          new BiologicalInfo { Value = 6428600, ColonyRange = 150 } },

            { "AMPHORA PLANT",          new BiologicalInfo { Value = 3626400, ColonyRange = 0   } },

            { "AUREUM BRAIN TREE",      new BiologicalInfo { Value = 3565100, ColonyRange = 0   } },

            { "BACTERIUM ACIES",        new BiologicalInfo { Value = 1500000, ColonyRange = 500 } },
            { "BACTERIUM ALCYONEUM",    new BiologicalInfo { Value = 3678100, ColonyRange = 500 } },
            { "BACTERIUM AURASUS",      new BiologicalInfo { Value = 2414700, ColonyRange = 500 } },
            { "BACTERIUM BULLARIS",     new BiologicalInfo { Value = 2766300, ColonyRange = 500 } },
            { "BACTERIUM CERBRUS",      new BiologicalInfo { Value = 3732200, ColonyRange = 500 } },
            { "BACTERIUM INFORMEM",     new BiologicalInfo { Value = 13114000, ColonyRange = 500 } },
            { "BACTERIUM NEBULUS",      new BiologicalInfo { Value = 9116600, ColonyRange = 500 } },
            { "BACTERIUM OMENTUM",      new BiologicalInfo { Value = 8226200, ColonyRange = 500 } },
            { "BACTERIUM SCOPULUM",     new BiologicalInfo { Value = 8633800, ColonyRange = 500 } },
            { "BACTERIUM TELA",         new BiologicalInfo { Value = 4173100, ColonyRange = 500 } },
            { "BACTERIUM VERRATA",      new BiologicalInfo { Value = 7177500, ColonyRange = 500 } },
            { "BACTERIUM VESICULA",     new BiologicalInfo { Value = 1725400, ColonyRange = 500 } },
            { "BACTERIUM VOLU",         new BiologicalInfo { Value = 12323000, ColonyRange = 500 } },

            { "BARK MOUNDS",            new BiologicalInfo { Value = 3350000, ColonyRange = 0   } },

            { "BLATTEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 3399800, ColonyRange = 0   } },
            { "BLATTEUM SINUOUS TUBERS",new BiologicalInfo { Value = 3425600, ColonyRange = 200 } },

            { "CACTOIDA CORTEXUM",      new BiologicalInfo { Value = 6844700, ColonyRange = 300 } },
            { "CACTOIDA LAPIS",         new BiologicalInfo { Value = 5044900, ColonyRange = 300 } },
            { "CACTOIDA PEPERATIS",     new BiologicalInfo { Value = 5044900, ColonyRange = 300 } },
            { "CACTOIDA PULLULANTA",    new BiologicalInfo { Value = 6844700, ColonyRange = 300 } },
            { "CACTOIDA VERMIS",        new BiologicalInfo { Value = 16777215, ColonyRange = 300 } },

            { "CLYPEUS LACRIMAM",       new BiologicalInfo { Value = 13114000, ColonyRange = 150 } },
            { "CLYPEUS MARGARITUS",     new BiologicalInfo { Value = 16777215, ColonyRange = 150 } },
            { "CLYPEUS SPECULUMI",      new BiologicalInfo { Value = 16777215, ColonyRange = 150 } },

            { "CONCHA AUREOLAS",        new BiologicalInfo { Value = 12323000, ColonyRange = 150 } },
            { "CONCHA BICONCAVIS",      new BiologicalInfo { Value = 16777215, ColonyRange = 150 } },
            { "CONCHA LABIATA",         new BiologicalInfo { Value = 4835200, ColonyRange = 150 } },
            { "CONCHA RENIBUS",         new BiologicalInfo { Value = 8133800, ColonyRange = 150 } },

            { "CROCEUM ANEMONE",        new BiologicalInfo { Value = 3399800, ColonyRange = 0 } },
            { "CRYSTALLINE SHARDS",     new BiologicalInfo { Value = 3626400, ColonyRange = 0 } },

            { "ELECTRICAE PLUMA",       new BiologicalInfo { Value = 10432700, ColonyRange = 100 } },
            { "ELECTRICAE RADIALEM",    new BiologicalInfo { Value = 10432700, ColonyRange = 100 } },

            { "FONTICULUA CAMPESTRIS",  new BiologicalInfo { Value = 1956100, ColonyRange = 500 } },
            { "FONTICULUA DIGITOS",     new BiologicalInfo { Value = 3928300, ColonyRange = 500 } },
            { "FONTICULUA FLUCTUS",     new BiologicalInfo { Value = 20000000, ColonyRange = 500 } },
            { "FONTICULUA LAPIDA",      new BiologicalInfo { Value = 6017400, ColonyRange = 500 } },
            { "FONTICULUA SEGMENTATUS", new BiologicalInfo { Value = 19010800, ColonyRange = 500 } },
            { "FONTICULUA UPUPAM",      new BiologicalInfo { Value = 9701800, ColonyRange = 500 } },

            { "FRUTEXA ACUS",           new BiologicalInfo { Value = 12323000, ColonyRange = 150 } },
            { "FRUTEXA COLLUM",         new BiologicalInfo { Value = 3645500, ColonyRange = 150 } },
            { "FRUTEXA FERA",           new BiologicalInfo { Value = 3632700, ColonyRange = 150 } },
            { "FRUTEXA FLABELLUM",      new BiologicalInfo { Value = 3936600, ColonyRange = 150 } },
            { "FRUTEXA FLAMMASIS",      new BiologicalInfo { Value = 15387600, ColonyRange = 150 } },
            { "FRUTEXA METALLICUM",     new BiologicalInfo { Value = 3632700, ColonyRange = 150 } },
            { "FRUTEXA SPONSAE",        new BiologicalInfo { Value = 10045400, ColonyRange = 150 } },

            { "FUMEROLA AQUATIS",       new BiologicalInfo { Value = 10432700, ColonyRange = 100 } },
            { "FUMEROLA CARBOSIS",      new BiologicalInfo { Value = 10432700, ColonyRange = 100 } },
            { "FUMEROLA EXTREMUS",      new BiologicalInfo { Value = 16777215, ColonyRange = 100 } },
            { "FUMEROLA NITRIS",        new BiologicalInfo { Value = 11982000, ColonyRange = 100 } },

            { "FUNGOIDA BULLARUM",      new BiologicalInfo { Value = 6896600, ColonyRange = 300 } },
            { "FUNGOIDA GELATA",        new BiologicalInfo { Value = 6346900, ColonyRange = 300 } },
            { "FUNGOIDA SETISIS",       new BiologicalInfo { Value = 3698100, ColonyRange = 300 } },
            { "FUNGOIDA STABITIS",      new BiologicalInfo { Value = 5355000, ColonyRange = 300 } },

            { "GYPSEEUM BRAIN TREE",        new BiologicalInfo { Value = 3565100, ColonyRange = 0 } },
            { "LINDIGOTICUM BRAIN TREE",    new BiologicalInfo { Value = 3565100, ColonyRange = 0 } },
            { "LINDIGOTICUM SINUOUS TUBERS",new BiologicalInfo { Value = 3425600, ColonyRange = 0 } },
            { "LIVIDUM BRAIN TREE",         new BiologicalInfo { Value = 3565100, ColonyRange = 0 } },
            { "LUTEOLUM ANEMONE",           new BiologicalInfo { Value = 3399800, ColonyRange = 0 } },

            { "OSSEUS CORNIBUS",        new BiologicalInfo { Value = 3369700, ColonyRange = 800 } },
            { "OSSEUS DISCUS",          new BiologicalInfo { Value = 16777215, ColonyRange = 800 } },
            { "OSSEUS FRACTUS",         new BiologicalInfo { Value = 7365300, ColonyRange = 800 } },
            { "OSSEUS PELLEBANTUS",     new BiologicalInfo { Value = 14698600, ColonyRange = 800 } },
            { "OSSEUS PUMICE",          new BiologicalInfo { Value = 6085800, ColonyRange = 800 } },
            { "OSSEUS SPIRALIS",        new BiologicalInfo { Value = 4919000, ColonyRange = 800 } },

            { "OSTRINUM BRAIN TREE",            new BiologicalInfo { Value =3565100, ColonyRange = 0 } },
            { "PRASINUM BIOLUMINESCENT ANEMONE",new BiologicalInfo { Value =3399800, ColonyRange = 0 } },
            { "PRASINUM SINUOUS TUBERS",        new BiologicalInfo { Value =3425600, ColonyRange = 0 } },
            { "PUNICEUM ANEMONE",               new BiologicalInfo { Value =3399800, ColonyRange = 0 } },
            { "PUNICEUM BRAIN TREE",            new BiologicalInfo { Value =3565100, ColonyRange = 0 } },

            { "RECEPTA CONDITIVUS",     new BiologicalInfo { Value = 16777215, ColonyRange = 150 } },
            { "RECEPTA DELTAHEDRONIX",  new BiologicalInfo { Value = 16777215, ColonyRange = 150 } },
            { "RECEPTA UMBRUX",         new BiologicalInfo { Value = 16777215, ColonyRange = 150 } },

            { "ROSEUM ANEMONE",                 new BiologicalInfo { Value = 3399800, ColonyRange = 0 } },
            { "ROSEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 3399800, ColonyRange = 0 } },
            { "ROSEUM BRAIN TREE",              new BiologicalInfo { Value = 3565100, ColonyRange = 0 } },
            { "ROSEUM SINUOUS TUBERS",          new BiologicalInfo { Value = 111300, ColonyRange = 0 } },
            { "RUBEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 3399800, ColonyRange = 0 } },

            { "STRATUM ARANEAMUS",      new BiologicalInfo { Value = 4989600, ColonyRange = 500 } },
            { "STRATUM CUCUMISIS",      new BiologicalInfo { Value = 16777215, ColonyRange = 500 } },
            { "STRATUM EXCUTITUS",      new BiologicalInfo { Value = 4989600, ColonyRange = 500 } },
            { "STRATUM FRIGUS",         new BiologicalInfo { Value = 5287900, ColonyRange = 500 } },
            { "STRATUM LAMINAMUS",      new BiologicalInfo { Value = 5523100, ColonyRange = 500 } },
            { "STRATUM LIMAXUS",        new BiologicalInfo { Value = 3152500, ColonyRange = 500 } },
            { "STRATUM PALEAS",         new BiologicalInfo { Value = 3152500, ColonyRange = 500 } },
            { "STRATUM TECTONICAS",     new BiologicalInfo { Value = 19010800, ColonyRange = 500 } },

            { "TUBUS CAVAS",            new BiologicalInfo { Value = 16777215, ColonyRange = 800 } },
            { "TUBUS COMPAGIBUS",       new BiologicalInfo { Value = 12323000, ColonyRange = 800 } },
            { "TUBUS CONIFER",          new BiologicalInfo { Value = 4936300, ColonyRange = 800 } },
            { "TUBUS ROSARIUM",         new BiologicalInfo { Value = 5287900, ColonyRange = 800 } },
            { "TUBUS SORORIBUS",        new BiologicalInfo { Value = 9701800, ColonyRange = 800 } },

            { "TUSSOCK ALBATA",         new BiologicalInfo { Value = 6230500, ColonyRange = 200 } },
            { "TUSSOCK CAPILLUM",       new BiologicalInfo { Value = 11383900, ColonyRange = 200 } },
            { "TUSSOCK CAPUTUS",        new BiologicalInfo { Value = 6557900, ColonyRange = 200 } },
            { "TUSSOCK CATENA",         new BiologicalInfo { Value = 3864400, ColonyRange = 200 } },
            { "TUSSOCK CULTRO",         new BiologicalInfo { Value = 3864400, ColonyRange = 200 } },
            { "TUSSOCK DIVISA",         new BiologicalInfo { Value = 3864400, ColonyRange = 200 } },
            { "TUSSOCK IGNIS",          new BiologicalInfo { Value = 4004600, ColonyRange = 200 } },
            { "TUSSOCK PENNATA",        new BiologicalInfo { Value = 9868700, ColonyRange = 200 } },
            { "TUSSOCK PENNATIS",       new BiologicalInfo { Value = 1832900, ColonyRange = 200 } },
            { "TUSSOCK PROPAGITO",      new BiologicalInfo { Value = 2194200, ColonyRange = 200 } },
            { "TUSSOCK SERRATI",        new BiologicalInfo { Value = 7958800, ColonyRange = 200 } },
            { "TUSSOCK STIGMASIS",      new BiologicalInfo { Value = 19010800, ColonyRange = 200 } },
            { "TUSSOCK TRITICUM",       new BiologicalInfo { Value = 12323000, ColonyRange = 200 } },
            { "TUSSOCK VENTUSA",        new BiologicalInfo { Value = 6193300, ColonyRange = 200 } },
            { "TUSSOCK VIRGAM",         new BiologicalInfo { Value = 16777215, ColonyRange = 200 } },

            { "VIOLACEUM SINUOUS TUBERS",   new BiologicalInfo { Value = 3425600, ColonyRange = 0 } },
            { "VIRIDE BRAIN TREE",          new BiologicalInfo { Value = 3565100, ColonyRange = 0 } },
            { "VIRIDE SINUOUS TUBERS",      new BiologicalInfo { Value = 3425600, ColonyRange = 0 } },
        };

        public static Dictionary<string, BiologicalInfo> BioValues { get => bioValues; }

        private ulong totalValue;
        public ulong TotalValue { get => totalValue; set { totalValue = value; OnPropertyChanged(); } }

        public ScannedBioData()
        {
            ObservableCollection<BiologicalData> biodata = LoadSave.LoadJson<ObservableCollection<BiologicalData>>(_saveFile);

            if (biodata is not null)
            {
                ScannedData = new(biodata);
            }

            AppSettings.Settings.SettingsInstance.SaveEvent += SettingsInstance_SaveEvent;

            //UpdateTotalValue();
        }

        ~ScannedBioData()
        {
            AppSettings.Settings.SettingsInstance.SaveEvent -= SettingsInstance_SaveEvent;
        }

        private void SettingsInstance_SaveEvent(object sender, System.EventArgs e)
        {
            _ = SaveState();
        }

        public void AddData(SystemBody systemBody, string species, string status, string timeStamp)
        {
            BiologicalData body = ScannedData.FirstOrDefault(x => string.Equals(x.SystemName, systemBody.SystemName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.BodyName, systemBody.BodyNameLocal, StringComparison.OrdinalIgnoreCase));

            if (body is null)
            {
                body = new BiologicalData
                {
                    SystemName = systemBody.SystemName,
                    BodyName = systemBody.BodyNameLocal,
                    BodyType = systemBody.AtmosphereDescrtiption,
                    AtmosphereType = systemBody.AtmosphereDescrtiption,
                    Volcanism = systemBody.Volcanism,
                    SurfaceGravity = systemBody.SurfaceGravity,
                    SurfacePressure = systemBody.SurfacePressure,
                    SurfaceTemp = systemBody.SurfaceTemp
                };

                ScannedData.AddToCollection(body);
            }

            BioData bioData = body.BodyBioData.FirstOrDefault(x => x.Species == species);

            if (bioData is not null)
            {
                bioData.Status = status;

                if (bioData.Status == "ANALYSED")
                {
                    TotalValue += (ulong)bioData.BioInfo.Value;
                }

                return;
            }

            bioData = new BioData
            {
                TimeStamp = timeStamp,
                Species = species,
                Status = status,
                BioInfo = bioValues[species]
            };

            body.BodyBioData.AddToCollection(bioData);
        }

        public void AddCodexData(SystemBody systemBody, string timeStamp, string name)
        {
            BiologicalData body = ScannedData.FirstOrDefault(x => string.Equals(x.SystemName, systemBody.SystemName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.BodyName, systemBody.BodyNameLocal, StringComparison.OrdinalIgnoreCase));

            string[] speciesAndVariant = name.Split(" - ");

            if (speciesAndVariant.Length < 2)
            {
                return;
            }

            if (body is null)
            {
                body = new BiologicalData
                {
                    SystemName = systemBody.SystemName,
                    BodyName = systemBody.BodyNameLocal,
                    BodyType = systemBody.BodyDescription,
                    AtmosphereType = systemBody.AtmosphereDescrtiption,
                    Volcanism = systemBody.Volcanism,
                    SurfaceGravity = systemBody.SurfaceGravity,
                    SurfacePressure = systemBody.SurfacePressure,
                    SurfaceTemp = systemBody.SurfaceTemp
                };

                ScannedData.AddToCollection(body);
            }
            BioData bioData = body.BodyBioData.FirstOrDefault(x => x.Species.Contains(speciesAndVariant[0], System.StringComparison.InvariantCultureIgnoreCase));

            if (bioData is not null)
            {
                bioData.Variant = $" - {speciesAndVariant[1]}";
                return;
            }

            bioData = new BioData
            {
                TimeStamp = timeStamp,
                Species = speciesAndVariant[0],
                Variant = $" - {speciesAndVariant[1]}",
                BioInfo = bioValues[speciesAndVariant[0]],
                Status = "CODEX ENTRY"
            };

            body.BodyBioData.AddToCollection(bioData);
        }

        internal void SellOrganic(SellOrganicDataEvent.SellOrganicDataEventArgs e)
        {
            foreach (var item in e.BioData)
            {
                TotalValue -= (ulong)item.Value;
            }

            if (TotalValue < 0)
            {
                TotalValue = 0;
            }
        }

        public void UpdateTotalValue()
        {
            ulong v = 0;

            foreach (BiologicalData bData in ScannedData)
            {
                v += (ulong)bData.GetBodyValue();
            }

            TotalValue = v;
        }

        public string GenerateCSV()
        {
            StringBuilder csvExport = new();
            _ = csvExport.AppendLine("Scan Date,Name,Variant,System Name,Body Name,Body Type,Atmosphere Type,Surface Pressure (pa),Surface Gravity,Surface Temp (K),Volcanism");

            foreach (BiologicalData biodata in ScannedData)
            {
                foreach (BioData bData in biodata.BodyBioData)
                {
                    _ = csvExport.AppendLine($"{bData.TimeStamp},{bData.Species},{bData.Variant.Replace(" - ", "")},{biodata.SystemName},{biodata.BodyName},{biodata.BodyType},{biodata.AtmosphereType},{biodata.SurfacePressure},{biodata.SurfaceGravity},{biodata.SurfaceTemp},{biodata.Volcanism}");
                }
            }

            return csvExport.ToString();
        }
        public void ResetData()
        {
            ScannedData.ClearCollection();
            //UpdateTotalValue();
            TotalValue = 0;
        }

        public bool SaveState()
        {
            return LoadSave.SaveJson(ScannedData, _saveFile);
        }
    }
}
