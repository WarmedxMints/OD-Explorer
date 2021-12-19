using ODExplorer.NavData;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.AppSettings
{
    /// <summary>
    /// Interaction logic for DisplaySettingsView.xaml
    /// </summary>
    public partial class DisplaySettingsView : Window
    {
        private Theme currentTHeme;
        public Settings Settings { get; private set; }

        public List<SystemInfo> CurrentSystem { get; set; } = new();
        public List<SystemInfo> SystemsInRoute { get; set; } = new();

        public DisplaySettingsView(Settings settings)
        {
            CreateSystems();
            Settings = settings;
            settings.CloneValues();
            currentTHeme = Settings.CurrentTheme;
            InitializeComponent();
        }
        private void CreateSystems()
        {
            CurrentSystem.Clear();

            SystemInfo system = new()
            {
                SystemName = "UNKOWN SYSTEM",
                DiscoveredBodiesCount = 10,
                IsKnownToEDSM = false,
                StarClass = "M",
                IsScoopable = true
            };

            system.Bodies.Clear();

            SystemBody body2 = new()
            {
                BodyNameLocal = "UNDISCOVERED",
                PlanetClass = EliteJournalReader.PlanetClass.IcyBody,
                Wasmapped = false,
                Landable = true,
                BiologicalSignals = 9,
                GeologicalSignals = 3,
                SurfaceGravity = 0.25,
                AtmosphereType = EliteJournalReader.AtmosphereClass.CarbonDioxide,
                SurfacePressure = 10000,
                SurfaceTemp = 310,
                DistanceFromArrivalLs = 180,
                MappedValue = 6180,
                Status = DiscoveryStatus.UnDiscovered,
                TerraformState = EliteJournalReader.TerraformState.None
            };

            system.Bodies.Add(body2);

            SystemBody body3 = new()
            {
                BodyNameLocal = "WORTH MAPPING",
                PlanetClass = EliteJournalReader.PlanetClass.EarthlikeBody,
                DistanceFromArrivalLs = 552,
                Status = DiscoveryStatus.WorthMapping,
                MappedValue = 4250180,
                Wasmapped = false,
                TerraformState = EliteJournalReader.TerraformState.Terraformed
            };

            system.Bodies.Add(body3);

            SystemBody body4 = new()
            {
                BodyNameLocal = "USER MAPPED",
                PlanetClass = EliteJournalReader.PlanetClass.WaterWorld,
                DistanceFromArrivalLs = 704,
                Status = DiscoveryStatus.MappedByUser,
                MappedValue = 2140560,
                Wasmapped = false,
                TerraformState = EliteJournalReader.TerraformState.Terraformable
            };

            system.Bodies.Add(body4);

            SystemBody body5 = new()
            {
                BodyNameLocal = "MEDIUM GRAVITY",
                PlanetClass = EliteJournalReader.PlanetClass.IcyBody,
                Wasmapped = true,
                Landable = true,
                BiologicalSignals = 1,
                GeologicalSignals = 0,
                SurfaceGravity = 1,
                AtmosphereType = EliteJournalReader.AtmosphereClass.CarbonDioxide,
                SurfacePressure = 100000,
                SurfaceTemp = 110,
                DistanceFromArrivalLs = 1200,
                MappedValue = 500,
                Status = DiscoveryStatus.Discovered,
            };

            system.Bodies.Add(body5);

            SystemBody body6 = new()
            {
                BodyNameLocal = "HIGH GRAVITY",
                PlanetClass = EliteJournalReader.PlanetClass.IcyBody,
                Wasmapped = true,
                Landable = true,
                BiologicalSignals = 0,
                GeologicalSignals = 2,
                SurfaceGravity = 19.52,
                AtmosphereType = EliteJournalReader.AtmosphereClass.CarbonDioxide,
                SurfacePressure = 1900000,
                SurfaceTemp = 3200,
                DistanceFromArrivalLs = 18452,
                MappedValue = 680,
                Status = DiscoveryStatus.Discovered
            };

            system.Bodies.Add(body6);

            SystemBody body7 = new()
            {
                BodyNameLocal = "BODY WITH RINGS",
                PlanetClass = EliteJournalReader.PlanetClass.SudarskyClassIGasGiant,
                Wasmapped = false,
                Landable = false,
                HasRings = true,
                RingReserves = EliteJournalReader.ReserveLevel.Pristine,
                DistanceFromArrivalLs = 42452,
                MappedValue = 38680,
                Status = DiscoveryStatus.Discovered
            };

            system.Bodies.Add(body7);

            SystemBody body1 = new()
            {
                BodyNameLocal = "DISCOVERED",
                StarType = EliteJournalReader.StarType.M,
                Status = DiscoveryStatus.Discovered,
                DistanceFromArrivalLs = 0,
                MappedValue = 1200
            };

            system.Bodies.Add(body1);

            CurrentSystem.Add(system);

            SystemInfo system2 = new()
            {
                SystemName = "KNOWN SYSTEM WITH NON-SCOOPABLE STAR",
                IsKnownToEDSM = true,
                StarClass = "TTS",
                IsScoopable = false,
                JumpDistanceToSystem = 62.82,
                JumpDistanceRemaining = 63,
                SysValue = 1200
            };

            SystemsInRoute.Add(system2);

            SystemInfo system3 = new()
            {
                SystemName = "KNOWN SYSTEM WITH NEUTRON STAR",
                IsKnownToEDSM = true,
                StarClass = "N",
                IsScoopable = false,
                JumpDistanceToSystem = 58,
                JumpDistanceRemaining = 121,
                SysValue = 1704070
            };

            SystemBody body8 = new()
            {
                BodyNameLocal = "VALUABLE BODY",
                PlanetClass = EliteJournalReader.PlanetClass.EdsmValuableBody,
                Status = DiscoveryStatus.Discovered,
                DistanceFromArrivalLs = 541,
                MappedValue = 1580270
            };

            system3.Bodies.Add(body8);
            SystemsInRoute.Add(system3);

            SystemInfo system4 = new()
            {
                SystemName = "KNOWN SYSTEM WITH SCOOPABLE MAIN STAR",
                IsKnownToEDSM = true,
                StarClass = "F",
                IsScoopable = true,
                JumpDistanceToSystem = 241.42,
                JumpDistanceRemaining = 362,
                SysValue = 56820
            };

            SystemsInRoute.Add(system4);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ExampleGrid.Items.Refresh();
        }

        private void CurrentSystemBodies_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid SystemBodiesGrid = (DataGrid)sender;

            foreach (DatagridLayout layout in Settings.ClonedValues.DisplaySettings.CSBColumnOrder)
            {
                int count = SystemBodiesGrid.Columns.Count == 0 ? 0 : SystemBodiesGrid.Columns.Count - 1;

                DataGridColumn column = SystemBodiesGrid.Columns.FirstOrDefault(x => (string)x.Header == layout.Header);

                if (column == default)
                {
                    continue;
                }

                column.DisplayIndex = (layout.DisplayIndex <= count) ? layout.DisplayIndex : count;
            }

            SystemBodiesGrid.ColumnReordered += CurrentSystemBodiesDataGrid_ColumnReordered;
        }

        private void CurrentSystemBodiesDataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            var SystemBodiesGrid = (DataGrid)sender;

            Settings.ClonedValues.DisplaySettings.CSBColumnOrder.Clear();

            foreach (DataGridColumn gridColumn in SystemBodiesGrid.Columns)
            {
                Settings.ClonedValues.DisplaySettings.CSBColumnOrder.Add(new DatagridLayout() { Header = (string)gridColumn.Header, DisplayIndex = gridColumn.DisplayIndex });
            }
        }

        private void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            //Save Values
            Settings.SetClonedValues();
            DialogResult = true;
        }

        private void Cancel_Btn_Click(object sender, RoutedEventArgs e)
        {
            Settings.CurrentTheme = currentTHeme;
            DialogResult = false;
        }
    }
}
