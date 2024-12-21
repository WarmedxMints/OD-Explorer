using System;

namespace ODExplorer.Models
{
    [Flags]
    public enum BodyNotification
    {
        None                    = 0,
        LandableTerraformable   = 1 << 0,
        LandableWithRings       = 1 << 1,
        LandableHighGravity     = 1 << 2,
        LandableLargeRadius     = 1 << 3,
        SmallPlanet             = 1 << 4,
        HighEccentricity        = 1 << 5,
        NestedMoon              = 1 << 6,
        FastRotation            = 1 << 7,
        FastOrbit               = 1 << 8,
        WideRings               = 1 << 9,
        DiverseLife             = 1 << 10,
        ShepherdMoon            = 1 << 11,
        BioSignals              = 1 << 12,
        GeoSignals              = 1 << 13,
        All                     = ~(-1 << 14)
    }

    public sealed class NotableNotificationOptions
    {
        public BodyNotification BodyNotifications { get; set; } = BodyNotification.All;
        public int DiverseLifeLimit { get; set; } = 9;
        public double HighSurfaceGravity { get; set; } = 29;
        public double LargeRadius { get; set; } = 18_000;
        public double SmallRadius { get; set; } = 300;
        public double EccentricityMin { get; set; } = 0.9;
        public double FastRotationMin { get; set; } = 8;
        public double FastOrbit { get; set; } = 8;
        public double RingWidthRadiusMultiplier { get; set; } = 5;

        /*  Values from EDDN
            SurfaceGravity  444.011112
            RadiusMax       28877854
            RadiusMin       1.04175
            Eccentricity    0.999968
            RotationPeriod  0.000127
            OrbitalPeriod   86.400002
        */
    }
}
