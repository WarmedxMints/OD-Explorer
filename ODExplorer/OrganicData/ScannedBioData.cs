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

            { "ALEOIDA ARCUS",          new BiologicalInfo { Value = 7252500, ColonyRange = 150 } },
            { "ALEOIDA CORONAMUS",      new BiologicalInfo { Value = 6284600, ColonyRange = 150 } },
            { "ALEOIDA GRAVIS",         new BiologicalInfo { Value = 12934900, ColonyRange = 150 } },
            { "ALEOIDA LAMINIAE",       new BiologicalInfo { Value = 3385200, ColonyRange = 150 } },
            { "ALEOIDA SPICA",          new BiologicalInfo { Value = 3385200, ColonyRange = 150 } },

            { "AMPHORA PLANT",          new BiologicalInfo { Value = 3626400, ColonyRange = 0   } },

            { "AUREUM BRAIN TREE",      new BiologicalInfo { Value = 1593700, ColonyRange = 0   } },

            { "BACTERIUM ACIES",        new BiologicalInfo { Value = 1000000, ColonyRange = 500 } },
            { "BACTERIUM ALCYONEUM",    new BiologicalInfo { Value = 1658500, ColonyRange = 500 } },
            { "BACTERIUM AURASUS",      new BiologicalInfo { Value = 1000000, ColonyRange = 500 } },
            { "BACTERIUM BULLARIS",     new BiologicalInfo { Value = 1152500, ColonyRange = 500 } },
            { "BACTERIUM CERBRUS",      new BiologicalInfo { Value = 1689800, ColonyRange = 500 } },
            { "BACTERIUM INFORMEM",     new BiologicalInfo { Value = 8418000, ColonyRange = 500 } },
            { "BACTERIUM NEBULUS",      new BiologicalInfo { Value = 5289900, ColonyRange = 500 } },
            { "BACTERIUM OMENTUM",      new BiologicalInfo { Value = 4638900, ColonyRange = 500 } },
            { "BACTERIUM SCOPULUM",     new BiologicalInfo { Value = 4934500, ColonyRange = 500 } },
            { "BACTERIUM TELA",         new BiologicalInfo { Value = 1949000, ColonyRange = 500 } },
            { "BACTERIUM VERRATA",      new BiologicalInfo { Value = 3897000, ColonyRange = 500 } },
            { "BACTERIUM VESICULA",     new BiologicalInfo { Value = 1000000, ColonyRange = 500 } },
            { "BACTERIUM VOLU",         new BiologicalInfo { Value = 7774700, ColonyRange = 500 } },

            { "BARK MOUNDS",            new BiologicalInfo { Value = 1471900, ColonyRange = 0   } },

            { "BLATTEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 1499900, ColonyRange = 0   } },
            { "BLATTEUM SINUOUS TUBERS",new BiologicalInfo { Value = 3425600, ColonyRange = 200 } },

            { "CACTOIDA CORTEXUM",      new BiologicalInfo { Value = 3667600, ColonyRange = 300 } },
            { "CACTOIDA LAPIS",         new BiologicalInfo { Value = 2483600, ColonyRange = 300 } },
            { "CACTOIDA PEPERATIS",     new BiologicalInfo { Value = 2483600, ColonyRange = 300 } },
            { "CACTOIDA PULLULANTA",    new BiologicalInfo { Value = 3667600, ColonyRange = 300 } },
            { "CACTOIDA VERMIS",        new BiologicalInfo { Value = 16202800, ColonyRange = 300 } },

            { "CAERULEUM SINUOUS TUBERS",new BiologicalInfo { Value = 1514500, ColonyRange = 0 } },

            { "CLYPEUS LACRIMAM",       new BiologicalInfo { Value = 8418000, ColonyRange = 150 } },
            { "CLYPEUS MARGARITUS",     new BiologicalInfo { Value = 11873200, ColonyRange = 150 } },
            { "CLYPEUS SPECULUMI",      new BiologicalInfo { Value = 16202800, ColonyRange = 150 } },

            { "CONCHA AUREOLAS",        new BiologicalInfo { Value = 7774700, ColonyRange = 150 } },
            { "CONCHA BICONCAVIS",      new BiologicalInfo { Value = 19010800, ColonyRange = 150 } },
            { "CONCHA LABIATA",         new BiologicalInfo { Value = 2352400, ColonyRange = 150 } },
            { "CONCHA RENIBUS",         new BiologicalInfo { Value = 4572400, ColonyRange = 150 } },

            { "CROCEUM ANEMONE",        new BiologicalInfo { Value = 1499900, ColonyRange = 0 } },
            { "CRYSTALLINE SHARDS",     new BiologicalInfo { Value = 1628800, ColonyRange = 0 } },

            { "ELECTRICAE PLUMA",       new BiologicalInfo { Value = 6284600, ColonyRange = 100 } },
            { "ELECTRICAE RADIALEM",    new BiologicalInfo { Value = 6284600, ColonyRange = 100 } },

            { "FONTICULUA CAMPESTRIS",  new BiologicalInfo { Value = 1000000, ColonyRange = 500 } },
            { "FONTICULUA DIGITOS",     new BiologicalInfo { Value = 1804100, ColonyRange = 500 } },
            { "FONTICULUA FLUCTUS",     new BiologicalInfo { Value = 20000000, ColonyRange = 500 } },
            { "FONTICULUA LAPIDA",      new BiologicalInfo { Value = 3111000, ColonyRange = 500 } },
            { "FONTICULUA SEGMENTATUS", new BiologicalInfo { Value = 19010800, ColonyRange = 500 } },
            { "FONTICULUA UPUPAM",      new BiologicalInfo { Value = 5727600, ColonyRange = 500 } },

            { "FRUTEXA ACUS",           new BiologicalInfo { Value = 7774700, ColonyRange = 150 } },
            { "FRUTEXA COLLUM",         new BiologicalInfo { Value = 1639800, ColonyRange = 150 } },
            { "FRUTEXA FERA",           new BiologicalInfo { Value = 1632500, ColonyRange = 150 } },
            { "FRUTEXA FLABELLUM",      new BiologicalInfo { Value = 1808900, ColonyRange = 150 } },
            { "FRUTEXA FLAMMASIS",      new BiologicalInfo { Value = 10326000, ColonyRange = 150 } },
            { "FRUTEXA METALLICUM",     new BiologicalInfo { Value = 1632500, ColonyRange = 150 } },
            { "FRUTEXA SPONSAE",        new BiologicalInfo { Value = 5988000, ColonyRange = 150 } },

            { "FUMEROLA AQUATIS",       new BiologicalInfo { Value = 6284600, ColonyRange = 100 } },
            { "FUMEROLA CARBOSIS",      new BiologicalInfo { Value = 6284600, ColonyRange = 100 } },
            { "FUMEROLA EXTREMUS",      new BiologicalInfo { Value = 16202800, ColonyRange = 100 } },
            { "FUMEROLA NITRIS",        new BiologicalInfo { Value = 7500900, ColonyRange = 100 } },

            { "FUNGOIDA BULLARUM",      new BiologicalInfo { Value = 3703200, ColonyRange = 300 } },
            { "FUNGOIDA GELATA",        new BiologicalInfo { Value = 3330300, ColonyRange = 300 } },
            { "FUNGOIDA SETISIS",       new BiologicalInfo { Value = 1670100, ColonyRange = 300 } },
            { "FUNGOIDA STABITIS",      new BiologicalInfo { Value = 2680300, ColonyRange = 300 } },

            { "GYPSEEUM BRAIN TREE",        new BiologicalInfo { Value = 1593700, ColonyRange = 0 } },
            { "LINDIGOTICUM BRAIN TREE",    new BiologicalInfo { Value = 1593700, ColonyRange = 0 } },
            { "LINDIGOTICUM SINUOUS TUBERS",new BiologicalInfo { Value = 3425600, ColonyRange = 0 } },
            { "LIVIDUM BRAIN TREE",         new BiologicalInfo { Value = 1593700, ColonyRange = 0 } },
            { "LUTEOLUM ANEMONE",           new BiologicalInfo { Value = 1499900, ColonyRange = 0 } },

            { "OSSEUS CORNIBUS",        new BiologicalInfo { Value = 1483000, ColonyRange = 800 } },
            { "OSSEUS DISCUS",          new BiologicalInfo { Value = 12934900, ColonyRange = 800 } },
            { "OSSEUS FRACTUS",         new BiologicalInfo { Value = 4027800, ColonyRange = 800 } },
            { "OSSEUS PELLEBANTUS",     new BiologicalInfo { Value = 9739000, ColonyRange = 800 } },
            { "OSSEUS PUMICE",          new BiologicalInfo { Value = 3156300, ColonyRange = 800 } },
            { "OSSEUS SPIRALIS",        new BiologicalInfo { Value = 2404700, ColonyRange = 800 } },

            { "OSTRINUM BRAIN TREE",            new BiologicalInfo { Value =1593700, ColonyRange = 0 } },
            { "PRASINUM BIOLUMINESCENT ANEMONE",new BiologicalInfo { Value =1499900, ColonyRange = 0 } },
            { "PRASINUM SINUOUS TUBERS",        new BiologicalInfo { Value =1514500, ColonyRange = 0 } },
            { "PUNICEUM ANEMONE",               new BiologicalInfo { Value =1499900, ColonyRange = 0 } },
            { "PUNICEUM BRAIN TREE",            new BiologicalInfo { Value =1593700, ColonyRange = 0 } },

            { "RECEPTA CONDITIVUS",     new BiologicalInfo { Value = 14313700, ColonyRange = 150 } },
            { "RECEPTA DELTAHEDRONIX",  new BiologicalInfo { Value = 16202800, ColonyRange = 150 } },
            { "RECEPTA UMBRUX",         new BiologicalInfo { Value = 12934900, ColonyRange = 150 } },

            { "ROSEUM ANEMONE",                 new BiologicalInfo { Value = 1499900, ColonyRange = 0 } },
            { "ROSEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 1499900, ColonyRange = 0 } },
            { "ROSEUM BRAIN TREE",              new BiologicalInfo { Value = 1593700, ColonyRange = 0 } },
            { "ROSEUM SINUOUS TUBERS",          new BiologicalInfo { Value = 111300, ColonyRange = 0 } },
            { "RUBEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 1499900, ColonyRange = 0 } },

            { "STRATUM ARANEAMUS",      new BiologicalInfo { Value = 2448900, ColonyRange = 500 } },
            { "STRATUM CUCUMISIS",      new BiologicalInfo { Value = 16202800, ColonyRange = 500 } },
            { "STRATUM EXCUTITUS",      new BiologicalInfo { Value = 2448900, ColonyRange = 500 } },
            { "STRATUM FRIGUS",         new BiologicalInfo { Value = 2637500, ColonyRange = 500 } },
            { "STRATUM LAMINAMUS",      new BiologicalInfo { Value = 2788300, ColonyRange = 500 } },
            { "STRATUM LIMAXUS",        new BiologicalInfo { Value = 1362000, ColonyRange = 500 } },
            { "STRATUM PALEAS",         new BiologicalInfo { Value = 1362000, ColonyRange = 500 } },
            { "STRATUM TECTONICAS",     new BiologicalInfo { Value = 19010800, ColonyRange = 500 } },

            { "TUBUS CAVAS",            new BiologicalInfo { Value = 11873200, ColonyRange = 800 } },
            { "TUBUS COMPAGIBUS",       new BiologicalInfo { Value = 7774700, ColonyRange = 800 } },
            { "TUBUS CONIFER",          new BiologicalInfo { Value = 2415500, ColonyRange = 800 } },
            { "TUBUS ROSARIUM",         new BiologicalInfo { Value = 2637500, ColonyRange = 800 } },
            { "TUBUS SORORIBUS",        new BiologicalInfo { Value = 5727600, ColonyRange = 800 } },

            { "TUSSOCK ALBATA",         new BiologicalInfo { Value = 3252500, ColonyRange = 200 } },
            { "TUSSOCK CAPILLUM",       new BiologicalInfo { Value = 7025800, ColonyRange = 200 } },
            { "TUSSOCK CAPUTUS",        new BiologicalInfo { Value = 3472400, ColonyRange = 200 } },
            { "TUSSOCK CATENA",         new BiologicalInfo { Value = 1766600, ColonyRange = 200 } },
            { "TUSSOCK CULTRO",         new BiologicalInfo { Value = 1766600, ColonyRange = 200 } },
            { "TUSSOCK DIVISA",         new BiologicalInfo { Value = 1766600, ColonyRange = 200 } },
            { "TUSSOCK IGNIS",          new BiologicalInfo { Value = 1849000, ColonyRange = 200 } },
            { "TUSSOCK PENNATA",        new BiologicalInfo { Value = 5853800, ColonyRange = 200 } },
            { "TUSSOCK PENNATIS",       new BiologicalInfo { Value = 1000000, ColonyRange = 200 } },
            { "TUSSOCK PROPAGITO",      new BiologicalInfo { Value = 1000000, ColonyRange = 200 } },
            { "TUSSOCK SERRATI",        new BiologicalInfo { Value = 4447100, ColonyRange = 200 } },
            { "TUSSOCK STIGMASIS",      new BiologicalInfo { Value = 19010800, ColonyRange = 200 } },
            { "TUSSOCK TRITICUM",       new BiologicalInfo { Value = 7774700, ColonyRange = 200 } },
            { "TUSSOCK VENTUSA",        new BiologicalInfo { Value = 3227700, ColonyRange = 200 } },
            { "TUSSOCK VIRGAM",         new BiologicalInfo { Value = 14313700, ColonyRange = 200 } },

            { "VIOLACEUM SINUOUS TUBERS",   new BiologicalInfo { Value = 3425600, ColonyRange = 0 } },
            { "VIRIDE BRAIN TREE",          new BiologicalInfo { Value = 1593700, ColonyRange = 0 } },
            { "VIRIDE SINUOUS TUBERS",      new BiologicalInfo { Value = 3425600, ColonyRange = 0 } },
        };

        public static BiologicalInfo BioValues(string species)
        { 
            if(bioValues.ContainsKey(species))
            {
                return bioValues[species];
            }

            return new BiologicalInfo() {  Value= 0,ColonyRange = 0 };
        }

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
                BioInfo = BioValues(species)
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
                BioInfo = BioValues(speciesAndVariant[0]),
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
