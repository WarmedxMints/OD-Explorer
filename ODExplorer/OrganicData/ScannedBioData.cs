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

        private const string bioValuesData = "{\n  \"ALEOIDA ARCUS\": 379300,\n  \"ALEOIDA CORONAMUS\": 339100,\n  \"ALEOIDA GRAVIS\": 596500,\n  \"ALEOIDA LAMINIAE\": 208900,\n  \"ALEOIDA SPICA\": 208900,\n  \"BACTERIUM ACIES\": 50000,\n  \"BACTERIUM ALCYONEUM\": 119500,\n  \"BACTERIUM AURASUS\": 78500,\n  \"BACTERIUM BULLARIS\": 89900,\n  \"BACTERIUM CERBRUS\": 121300,\n  \"BACTERIUM INFORMEM\": 426200,\n  \"BACTERIUM NEBULUS\": 296300,\n  \"BACTERIUM OMENTUM\": 267400,\n  \"BACTERIUM SCOPULUM\": 280600,\n  \"BACTERIUM TELA\": 135600,\n  \"BACTERIUM VERRATA\": 233300,\n  \"BACTERIUM VESICULA\": 56100,\n  \"BACTERIUM VOLU\": 400500,\n  \"CACTOIDA CORTEXUM\": 222500,\n  \"CACTOIDA LAPIS\": 164000,\n  \"CACTOIDA PEPERATIS\": 184000,\n  \"CACTOIDA PULLULANTA\": 222500,\n  \"CACTOIDA VERMIS\": 711500,\n  \"CLYPEUS LACRIMAM\": 426200,\n  \"CLYPEUS MARGARITUS\": 557800,\n  \"CLYPEUS SPECULUMI\": 711500,\n  \"CONCHA AUREOLAS\": 400500,\n  \"CONCHA BICONCAVIS\": 806300,\n  \"CONCHA LABIATA\": 157100,\n  \"CONCHA RENIBUS\": 264300,\n  \"ELECTRICAE PLUMA\": 339100,\n  \"ELECTRICAE RADIALEM\": 339100,\n  \"FONTICULUA CAMPESTRIS\": 63600,\n  \"FONTICULUA DIGITOS\": 127700,\n  \"FONTICULUA FLUCTUS\": 900000,\n  \"FONTICULUA LAPIDA\": 195600,\n  \"FONTICULUA SEGMENTATUS\": 806300,\n  \"FONTICULUA UPUPAM\": 315300,\n  \"FRUTEXA ACUS\": 400500,\n  \"FRUTEXA COLLUM\": 118500,\n  \"FRUTEXA FERA\": 118100,\n  \"FRUTEXA FLABELLUM\": 127900,\n  \"FRUTEXA FLAMMASIS\": 500100,\n  \"FRUTEXA METALLICUM\": 118100,\n  \"FRUTEXA SPONSAE\": 326500,\n  \"FUMEROLA AQUATIS\": 339100,\n  \"FUMEROLA CARBOSIS\": 339100,\n  \"FUMEROLA EXTREMUS\": 711500,\n  \"FUMEROLA NITRIS\": 389400,\n  \"FUNGOIDA BULLARUM\": 224100,\n  \"FUNGOIDA GELATA\": 206300,\n  \"FUNGOIDA SETISIS\": 120200,\n  \"FUNGOIDA STABITIS\": 174000,\n  \"OSSEUS CORNIBUS\": 109500,\n  \"OSSEUS DISCUS\": 596500,\n  \"OSSEUS FRACTUS\": 239400,\n  \"OSSEUS PELLEBANTUS\": 477700,\n  \"OSSEUS PUMICE\": 197800,\n  \"OSSEUS SPIRALIS\": 159900,\n  \"RECEPTA CONDITIVUS\": 645700,\n  \"RECEPTA DELTAHEDRONIX\": 711500,\n  \"RECEPTA UMBRUX\": 596500,\n  \"STRATUM ARANEAMUS\": 162200,\n  \"STRATUM CUCUMISIS\": 711500,\n  \"STRATUM EXCUTITUS\": 162200,\n  \"STRATUM FRIGUS\": 171900,\n  \"STRATUM LAMINAMUS\": 179500,\n  \"STRATUM LIMAXUS\": 102500,\n  \"STRATUM PALEAS\": 102500,\n  \"STRATUM TECTONICAS\": 806300,\n  \"TUBUS CAVAS\": 171900,\n  \"TUBUS COMPAGIBUS\": 102700,\n  \"TUBUS CONIFER\": 315300,\n  \"TUBUS ROSARIUM\": 400500,\n  \"TUBUS SORORIBUS\": 557800,\n  \"TUSSOCK ALBATA\": 202500,\n  \"TUSSOCK CAPILLUM\": 370000,\n  \"TUSSOCK CAPUTUS\": 213100,\n  \"TUSSOCK CATENA\": 125600,\n  \"TUSSOCK CULTRO\": 125600,\n  \"TUSSOCK DIVISA\": 125600,\n  \"TUSSOCK IGNIS\": 130100,\n  \"TUSSOCK PENNATA\": 320700,\n  \"TUSSOCK PENNATIS\": 59600,\n  \"TUSSOCK PROPAGITO\": 71300,\n  \"TUSSOCK SERRATI\": 258700,\n  \"TUSSOCK STIGMASIS\": 806300,\n  \"TUSSOCK TRITICUM\": 400500,\n  \"TUSSOCK VENTUSA\": 201300,\n  \"TUSSOCK VIRGAM\": 645700\n}";
        public ObservableCollection<BiologicalData> ScannedData { get; private set; } = new();

        private readonly Dictionary<string, int> bioValues = new();

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

            bioValues = JsonConvert.DeserializeObject<Dictionary<string, int>>(bioValuesData);

            foreach (BiologicalData bdata in ScannedData)
            {
                foreach (BioData d in bdata.BodyBioData)
                {
                    if (d.Value == 0)
                    {
                        KeyValuePair<string, int> val = bioValues.FirstOrDefault(x => x.Key.Equals(d.Species, System.StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrEmpty(val.Key))
                        {
                            d.Value = val.Value;
                        }
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
            BiologicalData body = ScannedData.FirstOrDefault(x => x.SystemName == systemBody.SystemName && x.BodyName == systemBody.BodyNameLocal);

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
                    Value = bioValues[species]
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
            BiologicalData body = ScannedData.FirstOrDefault(x => x.SystemName == systemBody.SystemName && x.BodyName == systemBody.BodyNameLocal) ;

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
                Value = bioValues[speciesAndVariant[0]],
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
