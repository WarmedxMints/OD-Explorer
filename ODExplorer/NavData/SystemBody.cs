using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.AppSettings;
using ODExplorer.Utils;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.NavData
{
    public enum DiscoveryStatus
    {
        Discovered,
        UnDiscovered,
        WorthMapping,
        MappedByUser
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
    public class SystemBody : PropertyChangeNotify
    {
        #region Contructors
        public SystemBody() { }
        public SystemBody(ScanEvent.ScanEventArgs e, bool Ody)
        {
            SystemName = e.StarSystem;
            SystemAddress = e.SystemAddress;
            BodyName = e.BodyName;
            BodyID = e.BodyID;
            DistanceFromArrivalLs = (int)e.DistanceFromArrivalLs;
            StarType = e.StarType;
            StellarMass = e.StellarMass ?? 0;
            PlanetClass = e.PlanetClass;
            MassEM = e.MassEM ?? 1;
            TerraformState = e.TerraformState;
            AtmosphereType = e.AtmosphereType;
            Landable = e.Landable ?? false;
            SurfaceGravity = e.SurfaceGravity ?? 0;
            SurfacePressure = e.SurfacePressure ?? 0;
            SurfaceTemp = (int)Math.Round(e.SurfaceTemperature ?? 0);
            WasDiscovered = e.WasDiscovered ?? false;
            Composition = e.Composition;
            AtmosphericComposition = e.AtmosphereComposition ?? Array.Empty<ScanItemComponent>(); ;
            Materials = e.Materials ?? Array.Empty<ScanItemComponent>(); ;
            Volcanism = string.IsNullOrEmpty(e.Volcanism) ? "No Volcanism" : e.Volcanism;
            TidalLock = e.TidalLock ?? false;
            Radius = e.Radius ?? 0;
            Rings = e.Rings ?? Array.Empty<PlanetRing>();
            if (e.Rings is not null && IsPlanet)
            {
                HasRings = e.Rings.Length > 0;
            }
            RingReserves = e.ReserveLevel;
            //Whan a body is mapped by the user a scan event is fired
            //For some reason that scan event can report the body as not mapped when it has been previously
            //So, if the user has just mapped a body, we ignore what the scan event has to say about this.
            if (!MappedByUser)
            {
                Wasmapped = e.WasMapped ?? false;
                CalcValues(Ody);
            }

            SurfaceGravity = Math.Round(SurfaceGravity / 10, 2);
            SetBodyNameLocal();

            UpdateStatus();
        }
        #endregion

        #region Body Info
        private PlanetRing[] rings = Array.Empty<PlanetRing>();
        private double radius;
        private bool tidalLock;
        private string volcanism;
        private StarType starType;
        private PlanetClass _planetClass;
        private double _gravity;
        private double _surfacePressure;
        private AtmosphereClass _atmosphereType = AtmosphereClass.None;
        private int _surfaceTemp;
        private int _mappedValue;
        private int _fssValue;
        private TerraformState terraformState;
        private ScanItemComponent[] materials = Array.Empty<ScanItemComponent>();
        private Composition composition;

        public PlanetRing[] Rings { get => rings; set { rings = value; OnPropertyChanged(); } }
        public double Radius { get => radius; set { radius = value; OnPropertyChanged(); } }
        public bool TidalLock { get => tidalLock; set { tidalLock = value; OnPropertyChanged(); } }
        public string Volcanism { get => volcanism; set { volcanism = value; OnPropertyChanged(); } }
        public ScanItemComponent[] Materials { get => materials; set { materials = value; OnPropertyChanged(); } }

        private ScanItemComponent[] atmosphericComposition = Array.Empty<ScanItemComponent>();
        public ScanItemComponent[] AtmosphericComposition { get => atmosphericComposition; set { atmosphericComposition = value; OnPropertyChanged(); } }

        public Composition Composition { get => composition; set { composition = value; OnPropertyChanged(); OnPropertyChanged("HasComposition"); } }
        public bool HasComposition { get { return Composition.Ice > 0.001 || Composition.Metal > 0.001 || Composition.Rock > 0.001; } }

        public StarType StarType
        {
            get => starType;
            set
            {
                starType = value;
                OnPropertyChanged();
                OnPropertyChanged("BodyDescription");
                OnPropertyChanged("IsStar");
                OnPropertyChanged("IsPlanet");
                OnPropertyChanged("IsNonBody");
            }
        }


        public PlanetClass PlanetClass
        {
            get => _planetClass;
            set
            {
                _planetClass = value;
                OnPropertyChanged();
                OnPropertyChanged("BodyDescription");
                OnPropertyChanged("IsStar");
                OnPropertyChanged("IsPlanet");
                OnPropertyChanged("IsNonBody");
                OnPropertyChanged("IsEDSMvb");
                OnPropertyChanged("PlanetImage");
            }
        }

        [IgnoreDataMember]
        public bool IsEDSMvb
        {
            get => PlanetClass == PlanetClass.EdsmValuableBody;
            set => OnPropertyChanged();
        }

        public double StellarMass { get => stellarMass; set { stellarMass = value; OnPropertyChanged(); } }
        public double MassEM { get => massEM; set { massEM = value; OnPropertyChanged(); } }
        public double SurfaceGravity
        {
            get { return _gravity; }
            set { _gravity = value; OnPropertyChanged(); }
        }
        public double SurfacePressure
        {
            get => _surfacePressure;
            set { _surfacePressure = value; OnPropertyChanged(); OnPropertyChanged("SurfacePressureString"); }
        }
        [IgnoreDataMember]
        public string SurfacePressureString
        {
            get =>
                //Convert Pa to Standard Atmosphere
                $"{_surfacePressure / 101325:N2} atm";
            set => OnPropertyChanged();
        }
        public AtmosphereClass AtmosphereType { get => _atmosphereType; set { _atmosphereType = value; OnPropertyChanged(); OnPropertyChanged("HasAtmosphere"); } }
        public int SurfaceTemp
        {
            get => _surfaceTemp;
            set { _surfaceTemp = value; OnPropertyChanged(); OnPropertyChanged("SurfaceTempString"); }
        }
        public string SurfaceTempString
        {
            get
            {
                //if (Landable == false)
                //{
                //    return "";
                //}

                return Settings.SettingsInstance.Value.TemperatureUnit switch
                {
                    Temperature.Kelvin => $"{_surfaceTemp:N0} K",
                    Temperature.Celsius => $"{_surfaceTemp - 273.15:N0} °C",
                    Temperature.Fahrenheit => $"{(_surfaceTemp - 273.15) * 9 / 5 + 32:N0} °F",
                    _ => "",
                };
            }
            set => OnPropertyChanged();
        }
        public int MappedValue
        {
            get => _mappedValue;
            set { _mappedValue = value; OnPropertyChanged(); OnPropertyChanged("ScanValue"); }
        }
        public int FssValue
        {
            get { return _fssValue; }
            set { _fssValue = value; OnPropertyChanged(); OnPropertyChanged("ScanValue"); }
        }
        public int BonusValue
        {
            get => bonusValue; set { bonusValue = value; OnPropertyChanged(); }
        }
        public int ScanValue
        {
            get
            {
                return _mappedByUser || IsStar ? _mappedValue : _fssValue;
            }
            set { }
        }
        public TerraformState TerraformState { get => terraformState; set { terraformState = value; OnPropertyChanged("Terraformable"); } }
        public string SystemName { get; set; }

        public long SystemAddress { get; set; }
        public string BodyName { get; set; }
        public string BodyNameLocal { get => bodyNameLocal; set { bodyNameLocal = value; OnPropertyChanged(); } }

        private long _bodyID;
        public long BodyID { get => _bodyID; set { _bodyID = value; OnPropertyChanged(); } }

        private int _distanceFromArrivalLs;
        public int DistanceFromArrivalLs
        {
            get => _distanceFromArrivalLs;
            set { _distanceFromArrivalLs = value; OnPropertyChanged(); }
        }
        #endregion

        #region Body Status
        private DiscoveryStatus _status;

        public DiscoveryStatus Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }

        private bool _landabe;

        public bool Landable
        {
            get => _landabe;
            set { _landabe = value; OnPropertyChanged(); }
        }

        public bool WasDiscovered { get; set; } = true;

        private bool _wasMapped = true;
        public bool Wasmapped
        {
            get { return !IsPlanet || _wasMapped; }
            set { _wasMapped = value; OnPropertyChanged(); }
        }

        [IgnoreDataMember]
        public bool WorthMapping
        {
            get => MappedValue >= Settings.SettingsInstance.Value.WorthMappingValue &&
                    (Settings.SettingsInstance.Value.WorthMappingDistance <= 0 || Settings.SettingsInstance.Value.WorthMappingDistance >= DistanceFromArrivalLs) &&
                    MappedByUser == false;
            set => OnPropertyChanged();
        }
        [IgnoreDataMember]
        public bool IsKnownToEDSM { get => WasDiscovered; }

        private bool _mappedByUser;
        public bool MappedByUser { get => _mappedByUser; set { _mappedByUser = value; OnPropertyChanged(); OnPropertyChanged("ScanValue"); } }

        public bool EffeicentMapped { get; set; }
        [IgnoreDataMember]
        public bool IsStar { get { return StarType != StarType.Unknown; } }
        [IgnoreDataMember]
        public bool IsPlanet { get { return PlanetClass != PlanetClass.Unknown; } }
        [IgnoreDataMember]
        public bool IsNonBody { get { return !IsStar && !IsPlanet; } }
        [IgnoreDataMember]
        public bool Terraformable { get { return TerraformState is TerraformState.Terraformable or TerraformState.Terraforming or TerraformState.Terraformed; } }
        [IgnoreDataMember]
        public bool HasAtmosphere { get { return AtmosphereType is not AtmosphereClass.None and not AtmosphereClass.NoAtmosphere and not AtmosphereClass.Unknown; } }
        private int _geologicalSignals { get; set; }
        public int GeologicalSignals
        {
            get => _geologicalSignals;
            set { _geologicalSignals = value; OnPropertyChanged(); }
        }

        private int _biologicalSignals { get; set; }
        public int BiologicalSignals
        {
            get => _biologicalSignals;
            set { _biologicalSignals = value; OnPropertyChanged(); }
        }

        private bool _hasRings;
        public bool HasRings { get => _hasRings; set { _hasRings = value; OnPropertyChanged(); } }

        private ReserveLevel _ringReserves;
        public ReserveLevel RingReserves { get => _ringReserves; set { _ringReserves = value; OnPropertyChanged(); } }

        private ToolTip toolTip;
        private double stellarMass;
        private double massEM;
        private int bonusValue;
        private string bodyNameLocal;

        [IgnoreDataMember]
        public ToolTip ToolTip
        {
            get
            {
                if (toolTip == null)
                {
                    toolTip = new ToolTip();

                    ToolTip.Content = new CustomControls.BodyToolTip(this);
                }

                return toolTip;
            }
            set { toolTip = value; OnPropertyChanged(); }
        }

        [IgnoreDataMember]
        public PlanetImage PlanetImage
        {
            get
            {
                switch (PlanetClass)
                {
                    case PlanetClass.MetalRichBody:
                    case PlanetClass.HighMetalContentBody:
                    case PlanetClass.RockyBody:
                    case PlanetClass.IcyBody:
                    case PlanetClass.RockyIceBody:
                        return PlanetImage.Planet;

                    case PlanetClass.EarthlikeBody:
                        return PlanetImage.Earth;

                    case PlanetClass.WaterWorld:
                    case PlanetClass.WaterGiant:
                    case PlanetClass.WaterGiantWithLife:
                        return PlanetImage.Water;

                    case PlanetClass.AmmoniaWorld:
                    case PlanetClass.GasGiantWithWaterBasedLife:
                    case PlanetClass.GasGiantWithAmmoniaBasedLife:
                    case PlanetClass.SudarskyClassIGasGiant:
                    case PlanetClass.SudarskyClassIIGasGiant:
                    case PlanetClass.SudarskyClassIIIGasGiant:
                    case PlanetClass.SudarskyClassIVGasGiant:
                    case PlanetClass.SudarskyClassVGasGiant:
                    case PlanetClass.HeliumRichGasGiant:
                    case PlanetClass.HeliumGasGiant:
                        return PlanetImage.Gas;

                    case PlanetClass.EdsmValuableBody:
                    case PlanetClass.Unknown:
                        return PlanetImage.Default;
                    default:
                        return PlanetImage.None;
                }
            }
        }
        #endregion
        public void UpdateFromDetailedScan(SystemBody e, bool Ody = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SystemName = e.SystemName;
                SystemAddress = e.SystemAddress;
                BodyName = e.BodyName;
                BodyID = e.BodyID;
                DistanceFromArrivalLs = e.DistanceFromArrivalLs;
                StarType = e.StarType;
                StellarMass = e.StellarMass;
                PlanetClass = e.PlanetClass;
                MassEM = e.MassEM;
                TerraformState = e.TerraformState;
                AtmosphereType = e.AtmosphereType;
                Landable = e.Landable;
                SurfaceGravity = e.SurfaceGravity;
                SurfacePressure = e.SurfacePressure;
                SurfaceTemp = e.SurfaceTemp;
                Composition = e.Composition;
                AtmosphericComposition = e.AtmosphericComposition;
                Materials = e.Materials;
                Volcanism = e.Volcanism;
                TidalLock = e.TidalLock;
                Radius = e.Radius;
                Rings = e.Rings;
                if (e.Rings is not null && IsPlanet)
                {
                    HasRings = e.Rings.Length > 0;
                }
                RingReserves = e.RingReserves;
                WasDiscovered = e.WasDiscovered;
                HasRings = e.HasRings;
                RingReserves = e.RingReserves;
                //Whan a body is mapped by the user a scan event is fired
                //For some reason that scan event can report the body as not mapped when it has been previously
                //So, if the user has just mapped a body, we ignore what the scan event has to say about this.
                if (!MappedByUser)
                {
                    Wasmapped = e.Wasmapped;
                }
                SetBodyNameLocal();
                CalcValues(Ody);
                UpdateStatus();
            });
        }

        public void SetBodyNameLocal()
        {
            if (string.IsNullOrEmpty(SystemName))
            {
                return;
            }

            BodyNameLocal = BodyName.StartsWith(SystemName, StringComparison.OrdinalIgnoreCase) && BodyName.Length > SystemName.Length
                ? BodyName.Remove(0, SystemName.Length + 1).ToUpperInvariant()
                : IsPlanet ? string.IsNullOrEmpty(BodyNameLocal) ? BodyName.ToUpperInvariant() : BodyNameLocal : "";
        }

        internal void UpdateFromEDSM(SystemBody e)
        {
            BodyID = e.BodyID;
            BodyName = e.BodyName;
            DistanceFromArrivalLs = e.DistanceFromArrivalLs;
            WasDiscovered = e.WasDiscovered;
            MappedValue = e.MappedValue;

            SetBodyNameLocal();
            UpdateStatus();
        }

        //Called when App settings are updated
        public void UpdateUI()
        {
            OnPropertyChanged("SurfaceTempString");
            OnPropertyChanged("PlanetImage");
            UpdateStatus();
        }

        public void UpdateStatus()
        {
            if (MappedByUser)
            {
                Status = DiscoveryStatus.MappedByUser;
                return;
            }

            //Only planets can be mapped so stars and non-bodies are not marked as so
            if (WorthMapping && IsPlanet && PlanetClass != PlanetClass.EdsmValuableBody)
            {
                Status = DiscoveryStatus.WorthMapping;
                return;
            }

            if (WasDiscovered == false)
            {
                Status = DiscoveryStatus.UnDiscovered;
                return;
            }

            Status = DiscoveryStatus.Discovered;
        }
        [IgnoreDataMember]
        public string BodyDescription
        {
            get
            {
                if (IsStar)
                {
                    return EnumDescriptionConverter.GetEnumDescription(StarType);
                }
                if (IsPlanet)
                {
                    return EnumDescriptionConverter.GetEnumDescription(PlanetClass);
                }

                return "-";
            }
        }


        public void CalcValues(bool odyssey)
        {
            if (IsStar)
            {
                FssValue = MappedValue = MathFunctions.GetStarValue(StarType, StellarMass);

                return;
            }
            if (IsPlanet)
            {
                FssValue = MathFunctions.GetBodyValue(this, odyssey, false, false);

                MappedValue = MathFunctions.GetPlanetValue(PlanetClass, MassEM, !WasDiscovered, !Wasmapped, Terraformable, odyssey, true, true);

                WorthMapping = (MappedValue >= Settings.SettingsInstance.Value.WorthMappingValue)
                               && (Settings.SettingsInstance.Value.WorthMappingDistance <= 0 || Settings.SettingsInstance.Value.WorthMappingDistance >= DistanceFromArrivalLs);
            }
        }
    }
}
