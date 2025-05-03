using System;
using System.ComponentModel;

namespace ODExplorer.Models
{
    public enum ActiveViewModel
    {
        Carto,
        ExoBiology,
        Settings,
        DisplaySettings,
        CartoDetails,
        Spansh,
        EdAstro
    }

    public enum CartoViewState
    {
        None = -1,
        DetailedView,
        ClassicView,
        HorizontalView,
        ExtendedBodyInfo,
        DetailedExo
    }

    public enum CartoDetailsViewState
    {
        None = -1,
        Unsold,
        Sold,
        Lost,
        Ignored
    }

    public enum PlanetImage
    {
        None,
        Default,
        Planet,
        Earth,
        Water,
        Gas,
    }

    public enum BodySortCategory
    {
        [Description("Mapped Value")]
        Value,
        [Description("Surface Gravity")]
        Gravity,
        [Description("Distance From Arrival")]
        Distance,
        [Description("Body Type")]
        Type,
        [Description("Body Name")]
        Name,
        [Description("Body ID")]
        BodyId,
        [Description("Biological Signals")]
        BioSignals,
        [Description("Geological Signals")]
        GeoSignals,
        [Description("Worth Mapping/Value")]
        WorthMappingValue,
        [Description("Worth Mapping/Distance")]
        WorthMappingDistance,
        [Description("No Sorting")]
        None
    }

    public enum Temperature
    {
        Kelvin,
        Celsius,
        Fahrenheit
    }

    public enum Pressure
    {
        Pascal,
        Atmosphere,
        Psi
    }

    public enum Distance
    {
        Kilometers,
        Miles
    }

    [Flags]
    public enum BodyInfoIconDisplay
    {
        None = 0,
        AtmosphereType = 1 << 0,
        SurfaceTemp = 1 << 1,
        SurfacePressure = 1 << 2,
        GeoSignals = 1 << 3,
        BioSignals = 1 << 4,
        Unmapped = 1 << 5,
        WasDiscovered = 1 << 6,
        Terraformable = 1 << 7,
        HasRings = 1 << 8,
        SurfaceGravity = 1 << 9,
        All = ~(-1 << 10)
    }

    public enum Jumponium
    {
        None,
        Basic,
        Standard,
        Premium
    }

    public enum PopOutMode
    {
        Normal,
        Opaque,
        Semitransparent,
        Transparent
    }
}
