using LoadSaveSystem;
using ODExplorer.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ODExplorer.GeologicalData
{
    public class ScannedGeoData
    {
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "GeoData.json");

        public ObservableCollection<GeoLogicalDataContainer> ScannedData { get; private set; } = new();

        public ScannedGeoData()
        {
            ObservableCollection<GeoLogicalDataContainer> geoData = LoadSave.LoadJson<ObservableCollection<GeoLogicalDataContainer>>(_saveFile);

            if (geoData is not null)
            {
                ScannedData = new(geoData);
            }

            AppSettings.Settings.SettingsInstance.SaveEvent += SettingsInstance_SaveEvent;
        }
        ~ScannedGeoData()
        {
            AppSettings.Settings.SettingsInstance.SaveEvent -= SettingsInstance_SaveEvent;
        }

        private void SettingsInstance_SaveEvent(object sender, System.EventArgs e)
        {
            _ = SaveState();
        }

        public void AddCodexData(string systemName, string bodyName, string name, int value)
        {
            GeoLogicalDataContainer body = ScannedData.FirstOrDefault(x => x.SystemName == systemName && x.BodyName == bodyName);

            if (body is null)
            {
                body = new GeoLogicalDataContainer
                {
                    SystemName = systemName,
                    BodyName = bodyName
                };

                ScannedData.AddToCollection(body);
            }

            GeoData geoData = body.BodyBioData.FirstOrDefault(x => x.GeoName.Contains(name, System.StringComparison.InvariantCultureIgnoreCase));

            if (geoData is not null)
            {
                return;
            }

            geoData = new GeoData
            {
                GeoName = name,
                Value = value
            };

            body.BodyBioData.AddToCollection(geoData);
        }

        public void ResetData()
        {
            ScannedData.ClearCollection();
        }

        public bool SaveState()
        {
            return LoadSave.SaveJson(ScannedData, _saveFile);
        }
    }
}
