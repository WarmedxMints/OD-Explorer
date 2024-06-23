using EliteJournalReader;
using EliteJournalReader.Events;
using LoadSaveSystem;
using ODExplorer.NavData;
using ODExplorer.Notifications;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using static System.Environment;

namespace ODExplorer.OrganicData
{
    public class ScannedBioData : PropertyChangeNotify
    {
        public static ScannedBioData Instance { get; set; }

        public event EventHandler<ApproachBodyEvent.ApproachBodyEventArgs> OnApproachBodyEvent;
#if PORTABLE
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "Data", "BioData.json");
#else
        private readonly string _saveFile = Path.Combine(GetFolderPath(SpecialFolder.CommonApplicationData), "ODExplorer", "BioData.json");
#endif

        public ObservableCollection<BiologicalData> ScannedData { get; private set; } = new();
        public BiologicalData CurrentScannedData { get; private set; }

        private static readonly Dictionary<string, BiologicalInfo> oldbioValues = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "ALBIDUM SINUOUS TUBERS", new BiologicalInfo { Value = 111300, ColonyRange = 100   } },

            { "ALEOIDA ARCUS",          new BiologicalInfo { Value = 7252500, ColonyRange = 150 } },
            { "ALEOIDA CORONAMUS",      new BiologicalInfo { Value = 6284600, ColonyRange = 150 } },
            { "ALEOIDA GRAVIS",         new BiologicalInfo { Value = 12934900, ColonyRange = 150 } },
            { "ALEOIDA LAMINIAE",       new BiologicalInfo { Value = 3385200, ColonyRange = 150 } },
            { "ALEOIDA SPICA",          new BiologicalInfo { Value = 3385200, ColonyRange = 150 } },

            { "AMPHORA PLANT",          new BiologicalInfo { Value = 3626400, ColonyRange = 100   } },

            { "AUREUM BRAIN TREE",      new BiologicalInfo { Value = 1593700, ColonyRange = 100   } },

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

            { "BARK MOUNDS",            new BiologicalInfo { Value = 1471900, ColonyRange = 100   } },

            { "BLATTEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 1499900, ColonyRange = 100   } },
            { "BLATTEUM SINUOUS TUBERS",new BiologicalInfo { Value = 3425600, ColonyRange = 200 } },

            { "CACTOIDA CORTEXUM",      new BiologicalInfo { Value = 3667600, ColonyRange = 300 } },
            { "CACTOIDA LAPIS",         new BiologicalInfo { Value = 2483600, ColonyRange = 300 } },
            { "CACTOIDA PEPERATIS",     new BiologicalInfo { Value = 2483600, ColonyRange = 300 } },
            { "CACTOIDA PULLULANTA",    new BiologicalInfo { Value = 3667600, ColonyRange = 300 } },
            { "CACTOIDA VERMIS",        new BiologicalInfo { Value = 16202800, ColonyRange = 300 } },

            { "CAERULEUM SINUOUS TUBERS",new BiologicalInfo { Value = 1514500, ColonyRange = 100 } },

            { "CLYPEUS LACRIMAM",       new BiologicalInfo { Value = 8418000, ColonyRange = 150 } },
            { "CLYPEUS MARGARITUS",     new BiologicalInfo { Value = 11873200, ColonyRange = 150 } },
            { "CLYPEUS SPECULUMI",      new BiologicalInfo { Value = 16202800, ColonyRange = 150 } },

            { "CONCHA AUREOLAS",        new BiologicalInfo { Value = 7774700, ColonyRange = 150 } },
            { "CONCHA BICONCAVIS",      new BiologicalInfo { Value = 19010800, ColonyRange = 150 } },
            { "CONCHA LABIATA",         new BiologicalInfo { Value = 2352400, ColonyRange = 150 } },
            { "CONCHA RENIBUS",         new BiologicalInfo { Value = 4572400, ColonyRange = 150 } },

            { "CROCEUM ANEMONE",        new BiologicalInfo { Value = 1499900, ColonyRange = 100 } },
            { "CRYSTALLINE SHARDS",     new BiologicalInfo { Value = 1628800, ColonyRange = 100 } },

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

            { "GYPSEEUM BRAIN TREE",        new BiologicalInfo { Value = 1593700, ColonyRange = 100 } },
            { "LINDIGOTICUM BRAIN TREE",    new BiologicalInfo { Value = 1593700, ColonyRange = 100 } },
            { "LINDIGOTICUM SINUOUS TUBERS",new BiologicalInfo { Value = 3425600, ColonyRange = 100 } },
            { "LIVIDUM BRAIN TREE",         new BiologicalInfo { Value = 1593700, ColonyRange = 100 } },
            { "LUTEOLUM ANEMONE",           new BiologicalInfo { Value = 1499900, ColonyRange = 100 } },

            { "OSSEUS CORNIBUS",        new BiologicalInfo { Value = 1483000, ColonyRange = 800 } },
            { "OSSEUS DISCUS",          new BiologicalInfo { Value = 12934900, ColonyRange = 800 } },
            { "OSSEUS FRACTUS",         new BiologicalInfo { Value = 4027800, ColonyRange = 800 } },
            { "OSSEUS PELLEBANTUS",     new BiologicalInfo { Value = 9739000, ColonyRange = 800 } },
            { "OSSEUS PUMICE",          new BiologicalInfo { Value = 3156300, ColonyRange = 800 } },
            { "OSSEUS SPIRALIS",        new BiologicalInfo { Value = 2404700, ColonyRange = 800 } },

            { "OSTRINUM BRAIN TREE",            new BiologicalInfo { Value =1593700, ColonyRange = 100 } },
            { "PRASINUM BIOLUMINESCENT ANEMONE",new BiologicalInfo { Value =1499900, ColonyRange = 100 } },
            { "PRASINUM SINUOUS TUBERS",        new BiologicalInfo { Value =1514500, ColonyRange = 100 } },
            { "PUNICEUM ANEMONE",               new BiologicalInfo { Value =1499900, ColonyRange = 100 } },
            { "PUNICEUM BRAIN TREE",            new BiologicalInfo { Value =1593700, ColonyRange = 100 } },

            { "RECEPTA CONDITIVUS",     new BiologicalInfo { Value = 14313700, ColonyRange = 150 } },
            { "RECEPTA DELTAHEDRONIX",  new BiologicalInfo { Value = 16202800, ColonyRange = 150 } },
            { "RECEPTA UMBRUX",         new BiologicalInfo { Value = 12934900, ColonyRange = 150 } },

            { "ROSEUM ANEMONE",                 new BiologicalInfo { Value = 1499900, ColonyRange = 100 } },
            { "ROSEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 1499900, ColonyRange = 100 } },
            { "ROSEUM BRAIN TREE",              new BiologicalInfo { Value = 1593700, ColonyRange = 100 } },
            { "ROSEUM SINUOUS TUBERS",          new BiologicalInfo { Value = 111300, ColonyRange = 100 } },
            { "RUBEUM BIOLUMINESCENT ANEMONE",  new BiologicalInfo { Value = 1499900, ColonyRange = 100 } },

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

            { "VIOLACEUM SINUOUS TUBERS",   new BiologicalInfo { Value = 3425600, ColonyRange = 100 } },
            { "VIRIDE BRAIN TREE",          new BiologicalInfo { Value = 1593700, ColonyRange = 100 } },
            { "VIRIDE SINUOUS TUBERS",      new BiologicalInfo { Value = 3425600, ColonyRange = 100 } },
        };

        private static readonly Dictionary<string, BiologicalInfo> bioValues = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "CODEX_ENT_TUBEABCD_02", new BiologicalInfo { Name = "ALBIDUM SINUOUS TUBERS", Value = 111300, ColonyRange = 100 } },

            { "CODEX_ENT_ALEOIDS_01", new BiologicalInfo { Name = "ALEOIDA ARCUS", Value = 7252500, ColonyRange = 150 } },
            { "CODEX_ENT_ALEOIDS_02_K", new BiologicalInfo { Name = "ALEOIDA CORONAMUS", Value = 6284600, ColonyRange = 150 } },
            { "CODEX_ENT_ALEOIDS_05_K", new BiologicalInfo { Name = "ALEOIDA GRAVIS", Value = 12934900, ColonyRange = 150 } },
            { "CODEX_ENT_ALEOIDS_04_F", new BiologicalInfo { Name = "ALEOIDA LAMINIAE", Value = 3385200, ColonyRange = 150 } },
            { "CODEX_ENT_ALEOIDS_03", new BiologicalInfo { Name = "ALEOIDA SPICA", Value = 3385200, ColonyRange = 150 } },

            { "CODEX_ENT_VENTS", new BiologicalInfo { Name = "AMPHORA PLANT", Value = 3626400, ColonyRange = 100 } },

            { "CODEX_ENT_SEEDEFGH_01", new BiologicalInfo { Name = "AUREUM BRAIN TREE", Value = 1593700, ColonyRange = 100 } },

            { "CODEX_ENT_BACTERIAL_04_ANTIMONY", new BiologicalInfo { Name = "BACTERIUM ACIES", Value = 1000000, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_06_G", new BiologicalInfo { Name = "BACTERIUM ALCYONEUM", Value = 1658500, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_01_M", new BiologicalInfo { Name = "BACTERIUM AURASUS", Value = 1000000, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_10_POLONIUM", new BiologicalInfo { Name = "BACTERIUM BULLARIS", Value = 1152500, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_12_M", new BiologicalInfo { Name = "BACTERIUM CERBRUS", Value = 1689800, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_08", new BiologicalInfo { Name = "BACTERIUM INFORMEM", Value = 8418000, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_02_ANTIMONY", new BiologicalInfo { Name = "BACTERIUM NEBULUS", Value = 5289900, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_11", new BiologicalInfo { Name = "BACTERIUM OMENTUM", Value = 4638900, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_03_NIOBIUM", new BiologicalInfo { Name = "BACTERIUM SCOPULUM", Value = 4934500, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_07_TIN", new BiologicalInfo { Name = "BACTERIUM TELA", Value = 1949000, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_13_TIN", new BiologicalInfo { Name = "BACTERIUM VERRATA", Value = 3897000, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_05_RUTHENIUM", new BiologicalInfo { Name = "BACTERIUM VESICULA", Value = 1000000, ColonyRange = 500 } },
            { "CODEX_ENT_BACTERIAL_09", new BiologicalInfo { Name = "BACTERIUM VOLU", Value = 7774700, ColonyRange = 500 } },

            { "CODEX_ENT_CONE", new BiologicalInfo { Name = "BARK MOUNDS", Value = 1471900, ColonyRange = 100 } },

            { "CODEX_ENT_SPHEREEFGH", new BiologicalInfo { Name = "BLATTEUM BIOLUMINESCENT ANEMONE", Value = 1499900, ColonyRange = 100 } },
            { "CODEX_ENT_TUBEEFGH", new BiologicalInfo { Name = "BLATTEUM SINUOUS TUBERS", Value = 3425600, ColonyRange = 200 } },

            { "CODEX_ENT_CACTOID_01_G", new BiologicalInfo { Name = "CACTOIDA CORTEXUM", Value = 3667600, ColonyRange = 300 } },
            { "CODEX_ENT_CACTOID_02", new BiologicalInfo { Name = "CACTOIDA LAPIS", Value = 2483600, ColonyRange = 300 } },
            { "CODEX_ENT_CACTOID_05", new BiologicalInfo { Name = "CACTOIDA PEPERATIS", Value = 2483600, ColonyRange = 300 } },
            { "CODEX_ENT_CACTOID_04_L", new BiologicalInfo { Name = "CACTOIDA PULLULANTA", Value = 3667600, ColonyRange = 300 } },
            { "CODEX_ENT_CACTOID_03", new BiologicalInfo { Name = "CACTOIDA VERMIS", Value = 16202800, ColonyRange = 300 } },

            { "CODEX_ENT_TUBEABCD_03", new BiologicalInfo { Name = "CAERULEUM SINUOUS TUBERS", Value = 1514500, ColonyRange = 100 } },

            { "CODEX_ENT_CLYPEUS_01_G", new BiologicalInfo { Name = "CLYPEUS LACRIMAM", Value = 8418000, ColonyRange = 150 } },
            { "CODEX_ENT_CLYPEUS_02_G", new BiologicalInfo { Name = "CLYPEUS MARGARITUS", Value = 11873200, ColonyRange = 150 } },
            { "CODEX_ENT_CLYPEUS_03", new BiologicalInfo { Name = "CLYPEUS SPECULUMI", Value = 16202800, ColonyRange = 150 } },

            { "CODEX_ENT_CONCHAS_02", new BiologicalInfo { Name = "CONCHA AUREOLAS", Value = 7774700, ColonyRange = 150 } },
            { "CODEX_ENT_CONCHAS_04_RUTHENIUM", new BiologicalInfo { Name = "CONCHA BICONCAVIS", Value = 19010800, ColonyRange = 150 } },
            { "CODEX_ENT_CONCHAS_03_A", new BiologicalInfo { Name = "CONCHA LABIATA", Value = 2352400, ColonyRange = 150 } },
            { "CODEX_ENT_CONCHAS_01", new BiologicalInfo { Name = "CONCHA RENIBUS", Value = 4572400, ColonyRange = 150 } },
            { "CODEX_ENT_SPHEREABCD_01", new BiologicalInfo { Name = "CROCEUM ANEMONE", Value = 1499900, ColonyRange = 100 } },

            { "CODEX_ENT_GROUND_STRUCT_ICE", new BiologicalInfo { Name = "CRYSTALLINE SHARDS", Value = 1628800, ColonyRange = 100 } },

            { "CODEX_ENT_ELECTRICAE_01_POLONIUM", new BiologicalInfo { Name = "ELECTRICAE PLUMA", Value = 6284600, ColonyRange = 1000 } },
            { "CODEX_ENT_ELECTRICAE_02_TELLURIUM", new BiologicalInfo { Name = "ELECTRICAE RADIALEM", Value = 6284600, ColonyRange = 1000 } },

            { "CODEX_ENT_FONTICULUS_02_M", new BiologicalInfo { Name = "FONTICULUA CAMPESTRIS", Value = 1000000, ColonyRange = 500 } },
            { "CODEX_ENT_FONTICULUS_06_K", new BiologicalInfo { Name = "FONTICULUA DIGITOS", Value = 1804100, ColonyRange = 500 } },
            { "CODEX_ENT_FONTICULUS_05_M", new BiologicalInfo { Name = "FONTICULUA FLUCTUS", Value = 20000000, ColonyRange = 500 } },
            { "CODEX_ENT_FONTICULUS_04_G", new BiologicalInfo { Name = "FONTICULUA LAPIDA", Value = 3111000, ColonyRange = 500 } },
            { "CODEX_ENT_FONTICULUS_01_M", new BiologicalInfo { Name = "FONTICULUA SEGMENTATUS", Value = 19010800, ColonyRange = 500 } },
            { "CODEX_ENT_FONTICULUS_03_T", new BiologicalInfo { Name = "FONTICULUA UPUPAM", Value = 5727600, ColonyRange = 500 } },

            { "CODEX_ENT_SHRUBS_02_G", new BiologicalInfo { Name = "FRUTEXA ACUS", Value = 7774700, ColonyRange = 150 } },
            { "CODEX_ENT_SHRUBS_07", new BiologicalInfo { Name = "FRUTEXA COLLUM", Value = 1639800, ColonyRange = 150 } },
            { "CODEX_ENT_SHRUBS_05", new BiologicalInfo { Name = "FRUTEXA FERA", Value = 1632500, ColonyRange = 150 } },
            { "CODEX_ENT_SHRUBS_01_G", new BiologicalInfo { Name = "FRUTEXA FLABELLUM", Value = 1808900, ColonyRange = 150 } },
            { "CODEX_ENT_SHRUBS_04", new BiologicalInfo { Name = "FRUTEXA FLAMMASIS", Value = 10326000, ColonyRange = 150 } },
            { "CODEX_ENT_SHRUBS_03_G", new BiologicalInfo { Name = "FRUTEXA METALLICUM", Value = 1632500, ColonyRange = 150 } },
            { "CODEX_ENT_SHRUBS_06_M", new BiologicalInfo { Name = "FRUTEXA SPONSAE", Value = 5988000, ColonyRange = 150 } },

            { "CODEX_ENT_FUMEROLAS_04_MOLYBDENUM", new BiologicalInfo { Name = "FUMEROLA AQUATIS", Value = 6284600, ColonyRange = 100 } },
            { "CODEX_ENT_FUMEROLAS_01_MOLYBDENUM", new BiologicalInfo { Name = "FUMEROLA CARBOSIS", Value = 6284600, ColonyRange = 100 } },
            { "CODEX_ENT_FUMEROLAS_02", new BiologicalInfo { Name = "FUMEROLA EXTREMUS", Value = 16202800, ColonyRange = 100 } },
            { "CODEX_ENT_FUMEROLAS_03_TUNGSTEN", new BiologicalInfo { Name = "FUMEROLA NITRIS", Value = 7500900, ColonyRange = 100 } },

            { "CODEX_ENT_FUNGOIDS_03_POLONIUM", new BiologicalInfo { Name = "FUNGOIDA BULLARUM", Value = 3703200, ColonyRange = 300 } },
            { "CODEX_ENT_FUNGOIDS_04", new BiologicalInfo { Name = "FUNGOIDA GELATA", Value = 3330300, ColonyRange = 300 } },
            { "CODEX_ENT_FUNGOIDS_01_POLONIUM", new BiologicalInfo { Name = "FUNGOIDA SETISIS", Value = 1670100, ColonyRange = 300 } },
            { "CODEX_ENT_FUNGOIDS_02_MOLYBDENUM", new BiologicalInfo { Name = "FUNGOIDA STABITIS", Value = 2680300, ColonyRange = 300 } },
            { "CODEX_ENT_SEEDABCD_01", new BiologicalInfo { Name = "GYPSEEUM BRAIN TREE", Value = 1593700, ColonyRange = 100 } },
            { "CODEX_ENT_SEEDEFGH_03", new BiologicalInfo { Name = "LINDIGOTICUM BRAIN TREE", Value = 1593700, ColonyRange = 100 } },
            { "CODEX_ENT_TUBEEFGH_01", new BiologicalInfo { Name = "LINDIGOTICUM SINUOUS TUBERS", Value = 3425600, ColonyRange = 100 } },
            { "CODEX_ENT_SEEDEFGH", new BiologicalInfo { Name = "LIVIDUM BRAIN TREE", Value = 1593700, ColonyRange = 100 } },
            { "CODEX_ENT_SPHERE", new BiologicalInfo { Name = "LUTEOLUM ANEMONE", Value = 1499900, ColonyRange = 100 } },
            { "CODEX_ENT_OSSEUS_05", new BiologicalInfo { Name = "OSSEUS CORNIBUS", Value = 1483000, ColonyRange = 800 } },
            { "CODEX_ENT_OSSEUS_02_NIOBIUM", new BiologicalInfo { Name = "OSSEUS DISCUS", Value = 12934900, ColonyRange = 800 } },
            { "CODEX_ENT_OSSEUS_01_G", new BiologicalInfo { Name = "OSSEUS FRACTUS", Value = 4027800, ColonyRange = 800 } },
            { "CODEX_ENT_OSSEUS_06", new BiologicalInfo { Name = "OSSEUS PELLEBANTUS", Value = 9739000, ColonyRange = 800 } },
            { "CODEX_ENT_OSSEUS_04_POLONIUM", new BiologicalInfo { Name = "OSSEUS PUMICE", Value = 3156300, ColonyRange = 800 } },
            { "CODEX_ENT_OSSEUS_03", new BiologicalInfo { Name = "OSSEUS SPIRALIS", Value = 2404700, ColonyRange = 800 } },
            { "CODEX_ENT_SEEDABCD_02", new BiologicalInfo { Name = "OSTRINUM BRAIN TREE", Value = 1593700, ColonyRange = 100 } },
            { "CODEX_ENT_SPHEREEFGH_02", new BiologicalInfo { Name = "PRASINUM BIOLUMINESCENT ANEMONE", Value = 1499900, ColonyRange = 100 } },
            { "CODEX_ENT_TUBEABCD_01", new BiologicalInfo { Name = "PRASINUM SINUOUS TUBERS", Value = 1514500, ColonyRange = 100 } },
            { "CODEX_ENT_SPHEREABCD_02", new BiologicalInfo { Name = "PUNICEUM ANEMONE", Value = 1499900, ColonyRange = 100 } },
            { "CODEX_ENT_SEEDEFGH_02", new BiologicalInfo { Name = "PUNICEUM BRAIN TREE", Value = 1593700, ColonyRange = 100 } },
            { "CODEX_ENT_RECEPTA_03_POLONIUM", new BiologicalInfo { Name = "RECEPTA CONDITIVUS", Value = 14313700, ColonyRange = 150 } },
            { "CODEX_ENT_RECEPTA_02_CADMIUM", new BiologicalInfo { Name = "RECEPTA DELTAHEDRONIX", Value = 16202800, ColonyRange = 150 } },
            { "CODEX_ENT_RECEPTA_01_M", new BiologicalInfo { Name = "RECEPTA UMBRUX", Value = 12934900, ColonyRange = 150 } },
            { "CODEX_ENT_SPHEREABCD_03", new BiologicalInfo { Name = "ROSEUM ANEMONE", Value = 1499900, ColonyRange = 100 } },
            { "CODEX_ENT_SPHEREEFGH_03", new BiologicalInfo { Name = "ROSEUM BIOLUMINESCENT ANEMONE", Value = 1499900, ColonyRange = 100 } },
            { "CODEX_ENT_SEED", new BiologicalInfo { Name = "ROSEUM BRAIN TREE", Value = 1593700, ColonyRange = 100 } },
            { "CODEX_ENT_TUBE", new BiologicalInfo { Name = "ROSEUM SINUOUS TUBERS", Value = 111300, ColonyRange = 100 } },
            { "CODEX_ENT_SPHEREEFGH_01", new BiologicalInfo { Name = "RUBEUM BIOLUMINESCENT ANEMONE", Value = 1499900, ColonyRange = 100 } },
            { "CODEX_ENT_STRATUM_04_F", new BiologicalInfo { Name = "STRATUM ARANEAMUS", Value = 2448900, ColonyRange = 500 } },
            { "CODEX_ENT_STRATUM_06_K", new BiologicalInfo { Name = "STRATUM CUCUMISIS", Value = 16202800, ColonyRange = 500 } },
            { "CODEX_ENT_STRATUM_01_K", new BiologicalInfo { Name = "STRATUM EXCUTITUS", Value = 2448900, ColonyRange = 500 } },
            { "CODEX_ENT_STRATUM_08", new BiologicalInfo { Name = "STRATUM FRIGUS", Value = 2637500, ColonyRange = 500 } },
            { "CODEX_ENT_STRATUM_03", new BiologicalInfo { Name = "STRATUM LAMINAMUS", Value = 2788300, ColonyRange = 500 } },
            { "CODEX_ENT_STRATUM_05_F", new BiologicalInfo { Name = "STRATUM LIMAXUS", Value = 1362000, ColonyRange = 500 } },
            { "CODEX_ENT_STRATUM_02_F", new BiologicalInfo { Name = "STRATUM PALEAS", Value = 1362000, ColonyRange = 500 } },
            { "CODEX_ENT_STRATUM_07_M", new BiologicalInfo { Name = "STRATUM TECTONICAS", Value = 19010800, ColonyRange = 500 } },
            { "CODEX_ENT_TUBUS_03_G", new BiologicalInfo { Name = "TUBUS CAVAS", Value = 11873200, ColonyRange = 800 } },
            { "CODEX_ENT_TUBUS_05_F", new BiologicalInfo { Name = "TUBUS COMPAGIBUS", Value = 7774700, ColonyRange = 800 } },
            { "CODEX_ENT_TUBUS_01_F", new BiologicalInfo { Name = "TUBUS CONIFER", Value = 2415500, ColonyRange = 800 } },
            { "CODEX_ENT_TUBUS_04", new BiologicalInfo { Name = "TUBUS ROSARIUM", Value = 2637500, ColonyRange = 800 } },
            { "CODEX_ENT_TUBUS_02", new BiologicalInfo { Name = "TUBUS SORORIBUS", Value = 5727600, ColonyRange = 800 } },
            { "CODEX_ENT_TUSSOCKS_08_G", new BiologicalInfo { Name = "TUSSOCK ALBATA", Value = 3252500, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_15_G", new BiologicalInfo { Name = "TUSSOCK CAPILLUM", Value = 7025800, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_11_G", new BiologicalInfo { Name = "TUSSOCK CAPUTUS", Value = 3472400, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_05", new BiologicalInfo { Name = "TUSSOCK CATENA", Value = 1766600, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_04", new BiologicalInfo { Name = "TUSSOCK CULTRO", Value = 1766600, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_10_G", new BiologicalInfo { Name = "TUSSOCK DIVISA", Value = 1766600, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_03_F", new BiologicalInfo { Name = "TUSSOCK IGNIS", Value = 1849000, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_01", new BiologicalInfo { Name = "TUSSOCK PENNATA", Value = 5853800, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_06", new BiologicalInfo { Name = "TUSSOCK PENNATIS", Value = 1000000, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_09", new BiologicalInfo { Name = "TUSSOCK PROPAGITO", Value = 1000000, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_07_G", new BiologicalInfo { Name = "TUSSOCK SERRATI", Value = 4447100, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_13", new BiologicalInfo { Name = "TUSSOCK STIGMASIS", Value = 19010800, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_12_K", new BiologicalInfo { Name = "TUSSOCK TRITICUM", Value = 7774700, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_02_M", new BiologicalInfo { Name = "TUSSOCK VENTUSA", Value = 3227700, ColonyRange = 200 } },
            { "CODEX_ENT_TUSSOCKS_14", new BiologicalInfo { Name = "TUSSOCK VIRGAM", Value = 14313700, ColonyRange = 200 } },
            { "CODEX_ENT_TUBEEFGH_02", new BiologicalInfo { Name = "VIOLACEUM SINUOUS TUBERS", Value = 3425600, ColonyRange = 100 } },
            { "CODEX_ENT_SEEDABCD_03", new BiologicalInfo { Name = "VIRIDE BRAIN TREE", Value = 1593700, ColonyRange = 100 } },
            { "CODEX_ENT_TUBEEFGH_03", new BiologicalInfo { Name = "VIRIDE SINUOUS TUBERS", Value = 3425600, ColonyRange = 100 } },
        };
        public static BiologicalInfo BioValues(string species)
        {
            foreach (var bio in bioValues.Values)
            {
                if (string.Equals(bio.Name, species, StringComparison.OrdinalIgnoreCase))
                {
                    return bio;
                }
            }

            return new BiologicalInfo() { Value = 0, ColonyRange = 100 };
        }

        private BiologicalInfo BioValues(ScanOrganicEvent.ScanOrganicEventArgs e)
        {
            foreach (var key in bioValues.Keys)
            {
                if (e.Variant.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return bioValues[key];
                }
            }

            return BioValues(e.Species_Localised);
        }

        private ulong totalValue;
        public ulong TotalValue { get => totalValue; set { totalValue = value; OnPropertyChanged(); } }

        StatusWatcher StatusWatcher { get; set; }
        public ScannedBioData()
        {
            Instance = this;

            StatusWatcher = new StatusWatcher(AppSettings.Settings.SettingsInstance.Value.JournalPath);
            StatusWatcher.StatusUpdated += StatusWatcher_StatusUpdated;
            StatusWatcher.StartWatching();
            ObservableCollection<BiologicalData> biodata = LoadSave.LoadJson<ObservableCollection<BiologicalData>>(_saveFile);

            if (biodata is not null)
            {
                ScannedData = new(biodata);
            }

            AppSettings.Settings.SettingsInstance.SaveEvent += SettingsInstance_SaveEvent;

            UpdateTotalValue();
        }

        private double currentLongitude;
        private double currentLatitude;
        private string currentBody = string.Empty;

        private void StatusWatcher_StatusUpdated(object sender, StatusFileEvent e)
        {
            if (string.IsNullOrEmpty(e.BodyName) && (string.IsNullOrEmpty(currentBody) == false))
            {
                if (CurrentScannedData is not null)
                {
                    foreach (var biological in CurrentScannedData.BodyBioData)
                    {
                        if (biological.ScanData.Any() == false)
                        {
                            continue;
                        }

                        foreach (var data in biological.ScanData)
                        {
                            data.DistanceToScan = "-";
                            data.FarEnoughFromScan = false;
                        }
                    }
                }
                CurrentScannedData = null;
                currentBody = string.Empty;
                return;
            }

            currentLatitude = e.Latitude;
            currentLongitude = e.Longitude;
            currentBody = e.BodyName;

            if (CurrentScannedData is null || string.Equals(CurrentScannedData.BodyNameFull, currentBody, StringComparison.OrdinalIgnoreCase) == false)
            {
                CurrentScannedData = ScannedData.Where(x => string.Equals(x.BodyNameFull, currentBody, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            }

            if (CurrentScannedData is not null)
            {
                foreach (var biological in CurrentScannedData.BodyBioData)
                {
                    if (biological.ScanData.Any() == false)
                    {
                        continue;
                    }

                    foreach (var data in biological.ScanData)
                    {
                        var dist = DistanceTo2(data.Latitude, data.Longtitude, currentLatitude, currentLongitude, e.PlanetRadius);
                        data.FarEnoughFromScan = biological.Status != "ANALYSED" && biological.BioInfo.ColonyRange < dist;
                        data.DistanceToScan = dist < 1000 ? $"{dist:N2}  m" : $"{dist / 1000:N2} Km";            
                    }

                    var allFarEnough = biological.ScanData.All((x) =>
                    {
                        return x.FarEnoughFromScan;
                    });

                    if (allFarEnough && biological.ScanData.Any(x => x.Notified == false))
                    {
                        foreach (var data in biological.ScanData)
                        {
                            data.Notified = true;
                        }
                        Notify("Minimum Distance Travelled", $"Over {biological.BioInfo.ColonyRange}m from sample of\n{biological.BioInfo.Name}");
                    }
                }
            }
        }

        public static double DistanceTo2(double latitude, double longitude, double targetLat, double targetLong, double radius)
        {
            /* https://www.movable-type.co.uk/scripts/latlong.html
             * const R = 6371e3; // metres
             * const φ1 = lat1 * Math.PI/180; // φ, λ in radians
             * const φ2 = lat2 * Math.PI/180;
             * const Δφ = (lat2-lat1) * Math.PI/180;
             * const Δλ = (lon2-lon1) * Math.PI/180;
             * const a = Math.sin(Δφ/2) * Math.sin(Δφ/2) +
             * Math.cos(φ1) * Math.cos(φ2) *
             * Math.sin(Δλ/2) * Math.sin(Δλ/2);
             * const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
             * const d = R * c; // in metres
             */

            double lat1 = Radians(latitude);
            double lat2 = Radians(targetLat);
            double deltaLong = Radians(targetLong - longitude);
            double deltaLat = Radians(targetLat - latitude);

            double a = (Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)) + Math.Cos(lat1) * Math.Cos(lat2) * (Math.Sin(deltaLong / 2) * Math.Sin(deltaLong / 2));
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radius * c;
        }

        public static double Radians(double value)
        {
            return value * (Math.PI / 180.0);
        }

        ~ScannedBioData()
        {
            StatusWatcher.StopWatching();
            AppSettings.Settings.SettingsInstance.SaveEvent -= SettingsInstance_SaveEvent;
        }

        private void SettingsInstance_SaveEvent(object sender, System.EventArgs e)
        {
            _ = SaveState();
        }

        private void Notify(string header, string message)
        {
            if (AppSettings.Settings.SettingsInstance.Value.EnableNotifications == false) { return; }

            MainWindow.Notifier.ShowCustomMessageOnMainThread(header, message, null);
        }

        public void AddData(SystemBody systemBody, string timeStamp, ScanOrganicEvent.ScanOrganicEventArgs e)
        {
            BiologicalData body = ScannedData.FirstOrDefault(x => string.Equals(x.SystemName, systemBody.SystemName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.BodyName, systemBody.BodyNameLocal, StringComparison.OrdinalIgnoreCase));

            if (body is null)
            {
                body = new BiologicalData
                {
                    SystemName = systemBody.SystemName,
                    BodyName = systemBody.BodyNameLocal,
                    BodyNameFull = systemBody.BodyName,
                    BodyType = systemBody.AtmosphereDescrtiption,
                    AtmosphereType = systemBody.AtmosphereDescrtiption,
                    Volcanism = systemBody.Volcanism,
                    SurfaceGravity = systemBody.SurfaceGravity,
                    SurfacePressure = systemBody.SurfacePressure,
                    SurfaceTemp = systemBody.SurfaceTemp
                };

                ScannedData.AddToCollection(body);
            }

            BioData bioData = body.BodyBioData.FirstOrDefault(x => e.Species_Localised.Contains(x.Species, StringComparison.OrdinalIgnoreCase));

            bioData ??= body.BodyBioData.FirstOrDefault(x => e.Genus_Localised.Contains(x.Species, StringComparison.OrdinalIgnoreCase));

            if (bioData is not null)
            {
                if (bioData.Status == "REPORTED")
                {
                    bioData.Species = e.Species_Localised.ToUpper();
                    bioData.Variant = e.Variant_Localised[(e.Species_Localised.Length)..].ToUpper();
                    bioData.CodexVariant = e.Variant;
                    bioData.Status = "CODEX ENTRY";
                    bioData.BioInfo = BioValues(e);
                }

                bioData.Status = e.ScanType;

                var scandata = bioData.ScanData.FirstOrDefault(x => string.Equals(x.ScanType, bioData.Status, StringComparison.OrdinalIgnoreCase));

                if (scandata is null && string.Equals(bioData.Status, "CODEX ENTRY") == false)
                {
                    BioScanData data = new()
                    {
                        Latitude = currentLatitude,
                        Longtitude = currentLongitude,
                        ScanType = bioData.Status,
                        Notified = false
                    };

                    switch (bioData.Status)
                    {

                        case "REPORTED":
                            break;
                        case "LOGGED":
                            Notify(bioData.BioInfo.Name, $"Sample 1 of 3\nRange : {bioData.BioInfo.ColonyRange}m");
                            break;
                        case "SAMPLED":
                            Notify(bioData.BioInfo.Name, $"Sample 2 of 3\nRange : {bioData.BioInfo.ColonyRange}m");
                            break;
                        case "ANALYSED":
                            data.Notified = true;
                            Notify(bioData.BioInfo.Name, $"Sample 3 of 3");
                            break;
                        default:
                            break;
                    }

                    bioData.ScanData.AddToCollection(data);
                }

                if (bioData.Status == "ANALYSED")
                {
                    UpdateTotalValue();
                }

                return;
            }

            bioData = new BioData
            {
                TimeStamp = timeStamp,
                Species = e.Species_Localised.ToUpper(),
                Variant = e.Variant_Localised[(e.Species_Localised.Length)..].ToUpper(),
                Status = e.ScanType,
                BioInfo = BioValues(e),
                CodexVariant = e.Variant
            };

            body.BodyBioData.AddToCollection(bioData);
        }

        public void AddCodexData(SystemBody systemBody, string timeStamp, string name, string codexName)
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
                    BodyNameFull = systemBody.BodyName,
                    BodyType = systemBody.BodyDescription,
                    AtmosphereType = systemBody.AtmosphereDescrtiption,
                    Volcanism = systemBody.Volcanism,
                    SurfaceGravity = systemBody.SurfaceGravity,
                    SurfacePressure = systemBody.SurfacePressure,
                    SurfaceTemp = systemBody.SurfaceTemp
                };

                ScannedData.AddToCollection(body);
            }
            BioData bioData = body.BodyBioData.FirstOrDefault(x => speciesAndVariant[0].Contains(x.Species, System.StringComparison.InvariantCultureIgnoreCase));

            if (bioData is not null)
            {
                if (bioData.Status == "REPORTED")
                {
                    bioData.Species = speciesAndVariant[0];
                    bioData.Variant = $" - {speciesAndVariant[1]}";
                    bioData.Status = "CODEX ENTRY";
                    bioData.BioInfo = BioValues(speciesAndVariant[0]);
                    bioData.CodexVariant = codexName;
                    Notify(bioData.BioInfo.Name, $"{bioData.BioInfo.Value:N0}\n{bioData.BioInfo.ColonyRange}m");
                }
                return;
            }

            bioData = new BioData
            {
                TimeStamp = timeStamp,
                Species = speciesAndVariant[0],
                Variant = $" - {speciesAndVariant[1]}",
                BioInfo = BioValues(speciesAndVariant[0]),
                Status = "CODEX ENTRY",
                CodexVariant = codexName
            };

            Notify(bioData.BioInfo.Name, $"{bioData.BioInfo.Value:N0}\n{bioData.BioInfo.ColonyRange}m");
            body.BodyBioData.AddToCollection(bioData);
        }

        public void AddDSSData(SystemBody systemBody, SAASignalsFoundEvent.SAASignalsFoundEventArgs e)
        {
            BiologicalData body = ScannedData.FirstOrDefault(x => string.Equals(x.SystemName, systemBody.SystemName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.BodyName, systemBody.BodyNameLocal, StringComparison.OrdinalIgnoreCase));

            if (body is null)
            {
                body = new BiologicalData
                {
                    SystemName = systemBody.SystemName,
                    BodyName = systemBody.BodyNameLocal,
                    BodyNameFull = systemBody.BodyName,
                    BodyType = systemBody.BodyDescription,
                    AtmosphereType = systemBody.AtmosphereDescrtiption,
                    Volcanism = systemBody.Volcanism,
                    SurfaceGravity = systemBody.SurfaceGravity,
                    SurfacePressure = systemBody.SurfacePressure,
                    SurfaceTemp = systemBody.SurfaceTemp
                };

                ScannedData.AddToCollection(body);
            }

            foreach (var signal in e.Genuses)
            {
                BioData bioData = body.BodyBioData.FirstOrDefault(x => x.Species.Contains(signal.Genus_Localised, StringComparison.OrdinalIgnoreCase));

                if (bioData is not null)
                {
                    continue;
                }

                bioData = new BioData
                {
                    TimeStamp = e.EliteTimeString,
                    Species = signal.Genus_Localised.ToUpper(),
                    Status = "Dss",
                };

                body.BodyBioData.AddToCollection(bioData);
            }

        }
        internal void SellOrganic(SellOrganicDataEvent.SellOrganicDataEventArgs e)
        {
            foreach (var item in e.BioData)
            {
                foreach (var system in ScannedData)
                {
                    foreach (var bioItem in system.BodyBioData)
                    {
                        if (bioItem.Sold || bioItem.Status != "ANALYSED")
                        {
                            continue;
                        }

                        var codexEntry = bioItem.CodexVariant;

                        if (string.IsNullOrEmpty(bioItem.Variant))
                        {
                            foreach (var keypair in ScannedBioData.bioValues)
                            {
                                if (bioItem.Species.StartsWith(keypair.Value.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    codexEntry = keypair.Key;
                                    break;
                                }
                            }
                        }

                        if (item.Variant.Contains(codexEntry))
                        {
                            bioItem.Sold = true;
                            continue;
                        }
                    }
                }
            }

            UpdateTotalValue();
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
                if (biodata is null)
                {
                    continue;
                }
                foreach (BioData bData in biodata.BodyBioData)
                {
                    if (bData is null)
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(bData.Variant))
                    {
                        continue;
                    }
                    try
                    {
                        _ = csvExport.AppendLine($"{bData.TimeStamp},{bData.Species},{bData.Variant.Replace(" - ", "")},{biodata.SystemName},{biodata.BodyName},{biodata.BodyType},{biodata.AtmosphereType},{biodata.SurfacePressure},{biodata.SurfaceGravity},{biodata.SurfaceTemp},{biodata.Volcanism}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
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

        internal void OnApproachBody(ApproachBodyEvent.ApproachBodyEventArgs e)
        {
            OnApproachBodyEvent?.Invoke(this, e);
        }
    }
}
