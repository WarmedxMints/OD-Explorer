using LoadSaveSystem;
using Newtonsoft.Json;
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
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "BioData.json");

        //private const string bioValuesData = "{\n  \"ALEOIDA ARCUS\": 379300,\n  \"ALEOIDA CORONAMUS\": 339100,\n  \"ALEOIDA GRAVIS\": 596500,\n  \"ALEOIDA LAMINIAE\": 208900,\n  \"ALEOIDA SPICA\": 208900,\n  \"BACTERIUM ACIES\": 50000,\n  \"BACTERIUM ALCYONEUM\": 119500,\n  \"BACTERIUM AURASUS\": 78500,\n  \"BACTERIUM BULLARIS\": 89900,\n  \"BACTERIUM CERBRUS\": 121300,\n  \"BACTERIUM INFORMEM\": 426200,\n  \"BACTERIUM NEBULUS\": 296300,\n  \"BACTERIUM OMENTUM\": 267400,\n  \"BACTERIUM SCOPULUM\": 280600,\n  \"BACTERIUM TELA\": 135600,\n  \"BACTERIUM VERRATA\": 233300,\n  \"BACTERIUM VESICULA\": 56100,\n  \"BACTERIUM VOLU\": 400500,\n  \"CACTOIDA CORTEXUM\": 222500,\n  \"CACTOIDA LAPIS\": 164000,\n  \"CACTOIDA PEPERATIS\": 184000,\n  \"CACTOIDA PULLULANTA\": 222500,\n  \"CACTOIDA VERMIS\": 711500,\n  \"CLYPEUS LACRIMAM\": 426200,\n  \"CLYPEUS MARGARITUS\": 557800,\n  \"CLYPEUS SPECULUMI\": 711500,\n  \"CONCHA AUREOLAS\": 400500,\n  \"CONCHA BICONCAVIS\": 806300,\n  \"CONCHA LABIATA\": 157100,\n  \"CONCHA RENIBUS\": 264300,\n  \"ELECTRICAE PLUMA\": 339100,\n  \"ELECTRICAE RADIALEM\": 339100,\n  \"FONTICULUA CAMPESTRIS\": 63600,\n  \"FONTICULUA DIGITOS\": 127700,\n  \"FONTICULUA FLUCTUS\": 900000,\n  \"FONTICULUA LAPIDA\": 195600,\n  \"FONTICULUA SEGMENTATUS\": 806300,\n  \"FONTICULUA UPUPAM\": 315300,\n  \"FRUTEXA ACUS\": 400500,\n  \"FRUTEXA COLLUM\": 118500,\n  \"FRUTEXA FERA\": 118100,\n  \"FRUTEXA FLABELLUM\": 127900,\n  \"FRUTEXA FLAMMASIS\": 500100,\n  \"FRUTEXA METALLICUM\": 118100,\n  \"FRUTEXA SPONSAE\": 326500,\n  \"FUMEROLA AQUATIS\": 339100,\n  \"FUMEROLA CARBOSIS\": 339100,\n  \"FUMEROLA EXTREMUS\": 711500,\n  \"FUMEROLA NITRIS\": 389400,\n  \"FUNGOIDA BULLARUM\": 224100,\n  \"FUNGOIDA GELATA\": 206300,\n  \"FUNGOIDA SETISIS\": 120200,\n  \"FUNGOIDA STABITIS\": 174000,\n  \"OSSEUS CORNIBUS\": 109500,\n  \"OSSEUS DISCUS\": 596500,\n  \"OSSEUS FRACTUS\": 239400,\n  \"OSSEUS PELLEBANTUS\": 477700,\n  \"OSSEUS PUMICE\": 197800,\n  \"OSSEUS SPIRALIS\": 159900,\n  \"RECEPTA CONDITIVUS\": 645700,\n  \"RECEPTA DELTAHEDRONIX\": 711500,\n  \"RECEPTA UMBRUX\": 596500,\n  \"STRATUM ARANEAMUS\": 162200,\n  \"STRATUM CUCUMISIS\": 711500,\n  \"STRATUM EXCUTITUS\": 162200,\n  \"STRATUM FRIGUS\": 171900,\n  \"STRATUM LAMINAMUS\": 179500,\n  \"STRATUM LIMAXUS\": 102500,\n  \"STRATUM PALEAS\": 102500,\n  \"STRATUM TECTONICAS\": 806300,\n  \"TUBUS CAVAS\": 171900,\n  \"TUBUS COMPAGIBUS\": 102700,\n  \"TUBUS CONIFER\": 315300,\n  \"TUBUS ROSARIUM\": 400500,\n  \"TUBUS SORORIBUS\": 557800,\n  \"TUSSOCK ALBATA\": 202500,\n  \"TUSSOCK CAPILLUM\": 370000,\n  \"TUSSOCK CAPUTUS\": 213100,\n  \"TUSSOCK CATENA\": 125600,\n  \"TUSSOCK CULTRO\": 125600,\n  \"TUSSOCK DIVISA\": 125600,\n  \"TUSSOCK IGNIS\": 130100,\n  \"TUSSOCK PENNATA\": 320700,\n  \"TUSSOCK PENNATIS\": 59600,\n  \"TUSSOCK PROPAGITO\": 71300,\n  \"TUSSOCK SERRATI\": 258700,\n  \"TUSSOCK STIGMASIS\": 806300,\n  \"TUSSOCK TRITICUM\": 400500,\n  \"TUSSOCK VENTUSA\": 201300,\n  \"TUSSOCK VIRGAM\": 645700\n}";
        private const string bioData = "{\n  \"ALEOIDA ARCUS\": {\n    \"Value\": 379300,\n    \"ColonyRange\": 150\n  },\n  \"ALEOIDA CORONAMUS\": {\n    \"Value\": 339100,\n    \"ColonyRange\": 150\n  },\n  \"ALEOIDA GRAVIS\": {\n    \"Value\": 596500,\n    \"ColonyRange\": 150\n  },\n  \"ALEOIDA LAMINIAE\": {\n    \"Value\": 208900,\n    \"ColonyRange\": 150\n  },\n  \"ALEOIDA SPICA\": {\n    \"Value\": 208900,\n    \"ColonyRange\": 150\n  },\n  \"BACTERIUM ACIES\": {\n    \"Value\": 50000,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM ALCYONEUM\": {\n    \"Value\": 119500,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM AURASUS\": {\n    \"Value\": 78500,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM BULLARIS\": {\n    \"Value\": 89900,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM CERBRUS\": {\n    \"Value\": 121300,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM INFORMEM\": {\n    \"Value\": 426200,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM NEBULUS\": {\n    \"Value\": 296300,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM OMENTUM\": {\n    \"Value\": 267400,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM SCOPULUM\": {\n    \"Value\": 280600,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM TELA\": {\n    \"Value\": 135600,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM VERRATA\": {\n    \"Value\": 233300,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM VESICULA\": {\n    \"Value\": 56100,\n    \"ColonyRange\": 500\n  },\n  \"BACTERIUM VOLU\": {\n    \"Value\": 400500,\n    \"ColonyRange\": 500\n  },\n  \"CACTOIDA CORTEXUM\": {\n    \"Value\": 222500,\n    \"ColonyRange\": 300\n  },\n  \"CACTOIDA LAPIS\": {\n    \"Value\": 164000,\n    \"ColonyRange\": 300\n  },\n  \"CACTOIDA PEPERATIS\": {\n    \"Value\": 184000,\n    \"ColonyRange\": 300\n  },\n  \"CACTOIDA PULLULANTA\": {\n    \"Value\": 222500,\n    \"ColonyRange\": 300\n  },\n  \"CACTOIDA VERMIS\": {\n    \"Value\": 711500,\n    \"ColonyRange\": 300\n  },\n  \"CLYPEUS LACRIMAM\": {\n    \"Value\": 426200,\n    \"ColonyRange\": 150\n  },\n  \"CLYPEUS MARGARITUS\": {\n    \"Value\": 557800,\n    \"ColonyRange\": 150\n  },\n  \"CLYPEUS SPECULUMI\": {\n    \"Value\": 711500,\n    \"ColonyRange\": 150\n  },\n  \"CONCHA AUREOLAS\": {\n    \"Value\": 400500,\n    \"ColonyRange\": 150\n  },\n  \"CONCHA BICONCAVIS\": {\n    \"Value\": 806300,\n    \"ColonyRange\": 150\n  },\n  \"CONCHA LABIATA\": {\n    \"Value\": 157100,\n    \"ColonyRange\": 150\n  },\n  \"CONCHA RENIBUS\": {\n    \"Value\": 264300,\n    \"ColonyRange\": 150\n  },\n  \"ELECTRICAE PLUMA\": {\n    \"Value\": 339100,\n    \"ColonyRange\": 1000\n  },\n  \"ELECTRICAE RADIALEM\": {\n    \"Value\": 339100,\n    \"ColonyRange\": 1000\n  },\n  \"FONTICULUA CAMPESTRIS\": {\n    \"Value\": 63600,\n    \"ColonyRange\": 500\n  },\n  \"FONTICULUA DIGITOS\": {\n    \"Value\": 127700,\n    \"ColonyRange\": 500\n  },\n  \"FONTICULUA FLUCTUS\": {\n    \"Value\": 900000,\n    \"ColonyRange\": 500\n  },\n  \"FONTICULUA LAPIDA\": {\n    \"Value\": 195600,\n    \"ColonyRange\": 500\n  },\n  \"FONTICULUA SEGMENTATUS\": {\n    \"Value\": 806300,\n    \"ColonyRange\": 500\n  },\n  \"FONTICULUA UPUPAM\": {\n    \"Value\": 315300,\n    \"ColonyRange\": 500\n  },\n  \"FRUTEXA ACUS\": {\n    \"Value\": 400500,\n    \"ColonyRange\": 150\n  },\n  \"FRUTEXA COLLUM\": {\n    \"Value\": 118500,\n    \"ColonyRange\": 150\n  },\n  \"FRUTEXA FERA\": {\n    \"Value\": 118100,\n    \"ColonyRange\": 150\n  },\n  \"FRUTEXA FLABELLUM\": {\n    \"Value\": 127900,\n    \"ColonyRange\": 150\n  },\n  \"FRUTEXA FLAMMASIS\": {\n    \"Value\": 500100,\n    \"ColonyRange\": 150\n  },\n  \"FRUTEXA METALLICUM\": {\n    \"Value\": 118100,\n    \"ColonyRange\": 150\n  },\n  \"FRUTEXA SPONSAE\": {\n    \"Value\": 326500,\n    \"ColonyRange\": 150\n  },\n  \"FUMEROLA AQUATIS\": {\n    \"Value\": 339100,\n    \"ColonyRange\": 100\n  },\n  \"FUMEROLA CARBOSIS\": {\n    \"Value\": 339100,\n    \"ColonyRange\": 100\n  },\n  \"FUMEROLA EXTREMUS\": {\n    \"Value\": 711500,\n    \"ColonyRange\": 100\n  },\n  \"FUMEROLA NITRIS\": {\n    \"Value\": 389400,\n    \"ColonyRange\": 100\n  },\n  \"FUNGOIDA BULLARUM\": {\n    \"Value\": 224100,\n    \"ColonyRange\": 300\n  },\n  \"FUNGOIDA GELATA\": {\n    \"Value\": 206300,\n    \"ColonyRange\": 300\n  },\n  \"FUNGOIDA SETISIS\": {\n    \"Value\": 120200,\n    \"ColonyRange\": 300\n  },\n  \"FUNGOIDA STABITIS\": {\n    \"Value\": 174000,\n    \"ColonyRange\": 300\n  },\n  \"OSSEUS CORNIBUS\": {\n    \"Value\": 109500,\n    \"ColonyRange\": 800\n  },\n  \"OSSEUS DISCUS\": {\n    \"Value\": 596500,\n    \"ColonyRange\": 800\n  },\n  \"OSSEUS FRACTUS\": {\n    \"Value\": 239400,\n    \"ColonyRange\": 800\n  },\n  \"OSSEUS PELLEBANTUS\": {\n    \"Value\": 477700,\n    \"ColonyRange\": 800\n  },\n  \"OSSEUS PUMICE\": {\n    \"Value\": 197800,\n    \"ColonyRange\": 800\n  },\n  \"OSSEUS SPIRALIS\": {\n    \"Value\": 159900,\n    \"ColonyRange\": 800\n  },\n  \"RECEPTA CONDITIVUS\": {\n    \"Value\": 645700,\n    \"ColonyRange\": 150\n  },\n  \"RECEPTA DELTAHEDRONIX\": {\n    \"Value\": 711500,\n    \"ColonyRange\": 150\n  },\n  \"RECEPTA UMBRUX\": {\n    \"Value\": 596500,\n    \"ColonyRange\": 150\n  },\n  \"STRATUM ARANEAMUS\": {\n    \"Value\": 162200,\n    \"ColonyRange\": 500\n  },\n  \"STRATUM CUCUMISIS\": {\n    \"Value\": 711500,\n    \"ColonyRange\": 500\n  },\n  \"STRATUM EXCUTITUS\": {\n    \"Value\": 162200,\n    \"ColonyRange\": 500\n  },\n  \"STRATUM FRIGUS\": {\n    \"Value\": 171900,\n    \"ColonyRange\": 500\n  },\n  \"STRATUM LAMINAMUS\": {\n    \"Value\": 179500,\n    \"ColonyRange\": 500\n  },\n  \"STRATUM LIMAXUS\": {\n    \"Value\": 102500,\n    \"ColonyRange\": 500\n  },\n  \"STRATUM PALEAS\": {\n    \"Value\": 102500,\n    \"ColonyRange\": 500\n  },\n  \"STRATUM TECTONICAS\": {\n    \"Value\": 806300,\n    \"ColonyRange\": 500\n  },\n  \"TUBUS CAVAS\": {\n    \"Value\": 171900,\n    \"ColonyRange\": 800\n  },\n  \"TUBUS COMPAGIBUS\": {\n    \"Value\": 102700,\n    \"ColonyRange\": 800\n  },\n  \"TUBUS CONIFER\": {\n    \"Value\": 315300,\n    \"ColonyRange\": 800\n  },\n  \"TUBUS ROSARIUM\": {\n    \"Value\": 400500,\n    \"ColonyRange\": 800\n  },\n  \"TUBUS SORORIBUS\": {\n    \"Value\": 557800,\n    \"ColonyRange\": 800\n  },\n  \"TUSSOCK ALBATA\": {\n    \"Value\": 202500,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK CAPILLUM\": {\n    \"Value\": 370000,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK CAPUTUS\": {\n    \"Value\": 213100,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK CATENA\": {\n    \"Value\": 125600,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK CULTRO\": {\n    \"Value\": 125600,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK DIVISA\": {\n    \"Value\": 125600,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK IGNIS\": {\n    \"Value\": 130100,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK PENNATA\": {\n    \"Value\": 320700,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK PENNATIS\": {\n    \"Value\": 59600,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK PROPAGITO\": {\n    \"Value\": 71300,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK SERRATI\": {\n    \"Value\": 258700,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK STIGMASIS\": {\n    \"Value\": 806300,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK TRITICUM\": {\n    \"Value\": 400500,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK VENTUSA\": {\n    \"Value\": 201300,\n    \"ColonyRange\": 200\n  },\n  \"TUSSOCK VIRGAM\": {\n    \"Value\": 645700,\n    \"ColonyRange\": 200\n  }\n}";

        public ObservableCollection<BiologicalData> ScannedData { get; private set; } = new();

        private readonly Dictionary<string, BiologicalInfo> bioValues = new();

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

            bioValues = JsonConvert.DeserializeObject<Dictionary<string, BiologicalInfo>>(bioData);

            foreach (BiologicalData bdata in ScannedData)
            {
                foreach (BioData d in bdata.BodyBioData)
                {
                    if (d.BioInfo == null)
                    {
                        KeyValuePair<string, BiologicalInfo> val = bioValues.FirstOrDefault(x => x.Key.Equals(d.Species, System.StringComparison.OrdinalIgnoreCase));

                        d.BioInfo = val.Value;
                    }
                }
            }
            UpdateTotalValue();
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
            }
            else
            {
                bioData = new BioData
                {
                    TimeStamp = timeStamp,
                    Species = species,
                    Status = status,
                    BioInfo = bioValues[species]
                };

                body.BodyBioData.AddToCollection(bioData);
            }

            if (bioData.Status == "ANALYSED")
            {
                UpdateTotalValue();
            }
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
                    _ = csvExport.AppendLine($"{bData.TimeStamp},{bData.Species},{bData.Variant.Replace(" - ","")},{biodata.SystemName},{biodata.BodyName},{biodata.BodyType},{biodata.AtmosphereType},{biodata.SurfacePressure},{biodata.SurfaceGravity},{biodata.SurfaceTemp},{biodata.Volcanism}");
                }
            }

            return csvExport.ToString();
        }
        public void ResetData()
        {
            ScannedData.ClearCollection();
            UpdateTotalValue();
        }

        public bool SaveState()
        {
            return LoadSave.SaveJson(ScannedData, _saveFile);
        }
    }
}
