using ODExplorer.Preset;

namespace ODExplorer.AppSettings.NoteableBody
{
    public class NoteableMenuItem : ODPresetMenuItem
    {
        public NoteablePlanetClass PlanetClass { get; set; }
        public NoteableAtmosphere Atmospheres { get; set; } = new();
        public NoteableVolcanism Volcanism { get; set; } = new();
        public DoubleRange EarthMasses { get; set; } = new(0, 10000);
        public IntRange DistanceFromArrival { get; set; } = new(0, 2000000);
        public DoubleRange Gravity { get; set; } = new(0, 100);
        public LandableStatus LandableStatusEnum { get; set; }
        public IntRange SurfaceTemp { get; set; } = new(0, 1000);
        public IntRange SurfacePressure { get; set; } = new(0, 10130000);
        public TerraformableStatus TerraformableEnum { get; set; }
        public int[] Signals { get; set; } = new int[] { 0, 0 };
        public int Value { get; set; } = 0;
    }
}
