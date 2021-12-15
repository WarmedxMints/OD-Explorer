using LoadSaveSystem;
using ODExplorer.Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ODExplorer.OrganicData
{
    public class ScannedBioData : PropertyChangeNotify
    {
        private readonly string _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "BioData.json");

        public ObservableCollection<BiologicalData> ScannedData { get; private set; } = new();

        public ScannedBioData()
        {
            ObservableCollection<BiologicalData> biodata = LoadSave.LoadJson<ObservableCollection<BiologicalData>>(_saveFile);

            if (biodata is not null)
            {
                ScannedData = new(biodata);
            }
            AppSettings.Settings.SettingsInstance.SaveEvent += SettingsInstance_SaveEvent;
        }

        ~ScannedBioData()
        {
            AppSettings.Settings.SettingsInstance.SaveEvent -= SettingsInstance_SaveEvent;
        }

        private void SettingsInstance_SaveEvent(object sender, System.EventArgs e)
        {
            _ = SaveState();
        }

        public void AddData(string systemName, string bodyName, string species, string status)
        {
            BiologicalData body = ScannedData.FirstOrDefault(x => x.SystemName == systemName && x.BodyName == bodyName);

            if (body is null)
            {
                body = new BiologicalData
                {
                    SystemName = systemName,
                    BodyName = bodyName
                };

                ScannedData.AddToCollection(body);
            }

            BioData bioData = body.BodyBioData.FirstOrDefault(x => x.Species == species);

            if (bioData is not null)
            {
                bioData.Status = status;
                return;
            }

            bioData = new BioData
            {
                Species = species,
                Status = status
            };

            body.BodyBioData.AddToCollection(bioData);
        }

        public void AddCodexData(string systemName, string bodyName, string name)
        {
            BiologicalData body = ScannedData.FirstOrDefault(x => x.SystemName == systemName && x.BodyName == bodyName);

            string[] speciesAndVariant = name.Split(" - ");

            if(speciesAndVariant.Length < 2)
            {
                return;
            }

            if (body is null)
            {
                body = new BiologicalData
                {
                    SystemName = systemName,
                    BodyName = bodyName
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
                Species = speciesAndVariant[0],
                Variant = $" - {speciesAndVariant[1]}",
                Status = "CODEX ENTRY"
            };

            body.BodyBioData.AddToCollection(bioData);
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
