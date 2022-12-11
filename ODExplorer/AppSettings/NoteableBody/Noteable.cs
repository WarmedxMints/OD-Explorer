using Newtonsoft.Json.Linq;
using ODExplorer.NavData;
using ODExplorer.OrganicData;
using ODExplorer.Preset;
using ODExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using static ODExplorer.NavData.SystemBody;

namespace ODExplorer.AppSettings.NoteableBody
{
    public class Noteable : PropertyChangeNotify
    {
        public Noteable()
        {
            NoteablePresets.OnMenuItemClick += NoteablePresets_OnMenuItemClick;
        }

        ~Noteable()
        {
            NoteablePresets.OnMenuItemClick -= NoteablePresets_OnMenuItemClick;
        }

        private void NoteablePresets_OnMenuItemClick(object sender, NoteableMenuItemClickArgs e)
        {
            PlanetClass = e.Preset.PlanetClass;
            Atmospheres.Indexes = e.Preset.Atmospheres.Indexes;
            Volcanism.Indexes = e.Preset.Volcanism.Indexes;
            EarthMasses.SetValues(e.Preset.EarthMasses);
            DistanceFromArrival.SetValues(e.Preset.DistanceFromArrival);
            Gravity.SetValues(e.Preset.Gravity);
            LandableStatusEnum = e.Preset.LandableStatusEnum;
            TerraformableEnum = e.Preset.TerraformableEnum;
            SurfaceTemp.SetValues(e.Preset.SurfaceTemp);
            SurfacePressure.SetValues(e.Preset.SurfacePressure);
            Signals = e.Preset.Signals;
            HighlightNoteable = true;
        }


        public NoteablePreset NoteablePresets { get; set; } = new();

        private NoteablePlanetClass planetClass = NoteablePlanetClass.Any;
        public NoteablePlanetClass PlanetClass { get => planetClass; set { planetClass = value; OnPropertyChanged(); } }

        private NoteableAtmosphere atmospheres = new();
        public NoteableAtmosphere Atmospheres { get => atmospheres; set { atmospheres = value; OnPropertyChanged(); } }

        private NoteableVolcanism volcanism = new();
        public NoteableVolcanism Volcanism { get => volcanism; set { volcanism = value; OnPropertyChanged(); } }

        private DoubleRange earthMasses = new(0, 10000);
        public DoubleRange EarthMasses { get => earthMasses; set { earthMasses = value; OnPropertyChanged(); } }

        private IntRange distanceFromArrival = new(0, 2000000);
        public IntRange DistanceFromArrival { get => distanceFromArrival; set { distanceFromArrival = value; OnPropertyChanged(); } }

        private DoubleRange gravity = new(0, 100);
        public DoubleRange Gravity { get => gravity; set { gravity = value; OnPropertyChanged(); } }

        private LandableStatus landableStatusEnum;
        public LandableStatus LandableStatusEnum { get => landableStatusEnum; set { landableStatusEnum = value; OnPropertyChanged(); } }

        private IntRange surfaceTemp = new(0, 1000);
        public IntRange SurfaceTemp { get => surfaceTemp; set { surfaceTemp = value; OnPropertyChanged(); } }

        private IntRange surfacePressure = new(0, 10130000);
        public IntRange SurfacePressure { get => surfacePressure; set { surfacePressure = value; OnPropertyChanged(); } }

        private TerraformableStatus terraformableEnum;
        public TerraformableStatus TerraformableEnum { get => terraformableEnum; set { terraformableEnum = value; OnPropertyChanged(); } }

        private int[] signals = new int[] { 0, 0 };
        public int[] Signals { get => signals; set { signals = value; OnPropertyChanged(); } }

        private bool highlightNoteable;
        public bool HighlightNoteable { get => highlightNoteable; set { highlightNoteable = value; OnPropertyChanged(); } }

        public bool IsNoteable(SystemBody body)
        {
            if (HighlightNoteable == false || body.IsPlanet == false || body.PlanetClass == EliteJournalReader.PlanetClass.EdsmValuableBody)
            {
                return false;
            }

            List<bool> ret = new();

            if ((int)PlanetClass > 0)
            {
                NoteablePlanetClass pClass = (NoteablePlanetClass)Enum.Parse(typeof(NoteablePlanetClass), body.PlanetClass.ToString());
                ret.Add(PlanetClass.HasFlag(pClass));
            }

            ret.Add(Atmospheres.StringoInfoSelected(body.AtmosphereDescrtiption));

            ret.Add(Volcanism.StringoInfoSelected(body.Volcanism));

            //MassEM
            EarthMasses.CheckInRange(ret, body.MassEM);

            //Distance
            DistanceFromArrival.CheckInRange(ret, body.DistanceFromArrivalLs);

            //Gravity
            Gravity.CheckInRange(ret, body.SurfaceGravity);

            //Is Landable
            switch (LandableStatusEnum)
            {
                case LandableStatus.Any:
                    break;
                case LandableStatus.Landable:
                    ret.Add(body.Landable);
                    break;
                case LandableStatus.NotLandable:
                    ret.Add(body.Landable == false);
                    break;
            }

            //Surface Temp
            SurfaceTemp.CheckInRange(ret, body.SurfaceTemp);

            //Surface Pressure
            SurfacePressure.CheckInRange(ret, (int)body.SurfacePressure);

            //Terraformable
            switch (TerraformableEnum)
            {
                case TerraformableStatus.Any:
                    break;
                case TerraformableStatus.Terraformable:
                    ret.Add(body.Terraformable);
                    break;
                case TerraformableStatus.NotTerraformable:
                    ret.Add(body.Terraformable == false);
                    break;
            }

            //signals
            //geo
            ret.Add(body.GeologicalSignals >= Signals[0]);
            //bio
            ret.Add(body.BiologicalSignals >= Signals[1]);

            return !ret.Contains(false);
        }

        public FoundSignals[] GetPresetMatches(SystemBody body)
        {
            if (body.IsPlanet == false || body.PlanetClass == EliteJournalReader.PlanetClass.EdsmValuableBody)
            {
                return Array.Empty<FoundSignals>();
            }

            List<FoundSignals> Items = new();

            foreach (var preset in NoteablePresets.ODPresetGroups)
            {
                foreach (var pset in preset.MenuItems)
                {

                    var p = ((JObject)pset).ToObject<ODPresetSubMenuItem>();

                    if (p is null)
                    {
                        continue;
                    }

                    var menuItem = p as ODPresetSubMenuItem;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var qas in menuItem.MenuItems)
                        {
                            var q = ((JObject)qas).ToObject<NoteableMenuItem>();

                            if (q is null)
                            {
                                continue;
                            }

                            List<bool> ret = CheckMenuItemMatch(body, q);

                            if (!ret.Contains(false))
                            {
                                var name = $"{menuItem.MenuTitle} {q.Header}".ToUpperInvariant();

                                if (ScannedBioData.BioValues.ContainsKey(name))
                                {
                                    q.Value = ScannedBioData.BioValues[name].Value;
                                }
                                Items.Add(new FoundSignals() { Name = name, Value = q.Value});
                            }
                        }
                    });
                }
            }

            foreach (var preset in NoteablePresets.CustomPresets)
            {
                if (preset is null)
                {
                    continue;
                }

                List<bool> ret = CheckMenuItemMatch(body, preset);

                if (!ret.Contains(false))
                {
                    if (ScannedBioData.BioValues.ContainsKey(preset.Header.ToUpperInvariant()))
                    {
                        preset.Value = ScannedBioData.BioValues[preset.Header.ToUpperInvariant()].Value;
                    }

                    //Items.Add(new FoundSignals();
                }
            }

            return Items.ToArray();
        }

        private List<bool> CheckMenuItemMatch(SystemBody body, NoteableMenuItem menuItem)
        {
            List<bool> ret = new();

            if ((int)PlanetClass > 0)
            {
                NoteablePlanetClass pClass = (NoteablePlanetClass)Enum.Parse(typeof(NoteablePlanetClass), body.PlanetClass.ToString());
                ret.Add(menuItem.PlanetClass.HasFlag(pClass));
            }

            ret.Add(menuItem.Atmospheres.StringoInfoSelected(body.AtmosphereDescrtiption));

            ret.Add(menuItem.Volcanism.StringoInfoSelected(body.Volcanism));

            //MassEM
            menuItem.EarthMasses.CheckInRange(ret, body.MassEM);

            //Distance
            menuItem.DistanceFromArrival.CheckInRange(ret, body.DistanceFromArrivalLs);

            //Gravity
            menuItem.Gravity.CheckInRange(ret, body.SurfaceGravity);

            //Is Landable
            switch (menuItem.LandableStatusEnum)
            {
                case LandableStatus.Any:
                    break;
                case LandableStatus.Landable:
                    ret.Add(body.Landable);
                    break;
                case LandableStatus.NotLandable:
                    ret.Add(body.Landable == false);
                    break;
            }

            //Surface Temp
            menuItem.SurfaceTemp.CheckInRange(ret, body.SurfaceTemp);

            //Surface Pressure
            menuItem.SurfacePressure.CheckInRange(ret, (int)body.SurfacePressure);

            //Terraformable
            switch (menuItem.TerraformableEnum)
            {
                case TerraformableStatus.Any:
                    break;
                case TerraformableStatus.Terraformable:
                    ret.Add(body.Terraformable);
                    break;
                case TerraformableStatus.NotTerraformable:
                    ret.Add(body.Terraformable == false);
                    break;
            }

            //signals
            //geo
            ret.Add(body.GeologicalSignals >= menuItem.Signals[0]);
            //bio
            ret.Add(body.BiologicalSignals >= menuItem.Signals[1]);
            return ret;
        }

        internal void AddCustomPreset(string inputText)
        {
            NoteableMenuItem menuItem = new()
            {
                Header = inputText,
                PlanetClass = planetClass,
                Atmospheres = new() { Indexes = atmospheres.Indexes },
                Volcanism = new() { Indexes = volcanism.Indexes },
                EarthMasses = new(0, 10000) { Minimun = earthMasses.Minimun, Maximum = earthMasses.Maximum, IsActive = earthMasses.IsActive },
                DistanceFromArrival = distanceFromArrival = new(0, 2000000) { Minimun = distanceFromArrival.Minimun, Maximum = distanceFromArrival.Maximum, IsActive = distanceFromArrival.IsActive },
                Gravity = gravity = new(0, 100) { Minimun = gravity.Minimun, Maximum = gravity.Maximum, IsActive = gravity.IsActive },
                LandableStatusEnum = landableStatusEnum,
                SurfaceTemp = surfaceTemp = new(0, 1000) { Minimun = surfaceTemp.Minimun, Maximum = surfaceTemp.Maximum, IsActive = surfaceTemp.IsActive },
                SurfacePressure = surfacePressure = new(0, 10130000) { Minimun = surfacePressure.Minimun, Maximum = surfacePressure.Maximum, IsActive = surfacePressure.IsActive },
                TerraformableEnum = terraformableEnum,
                Signals = new int[] { signals[0], signals[1] }
            };

            NoteablePresets.AddCustom(menuItem);
        }
    }
}
