using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.AppSettings;
using ODExplorer.Utils;
using System;
using System.Runtime.Serialization;
using System.Windows;

namespace ODExplorer.NavData
{
    public enum DiscoveryStatus
    {
        Discovered,
        UnDiscovered,
        WorthMapping,
        MappedByUser
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
            MassEM = e.MassEM ?? 0;
            TerraformState = e.TerraformState;
            SurfaceGravity = e.SurfaceGravity ?? 0;
            Landable = e.Landable ?? false;
            WasDiscovered = e.WasDiscovered ?? false;
            //Whan a body is mapped by the user a scan event is fired
            //For some reason that scan event can report the body as not mapped when it has been previously
            //So, if the user has just mapped a body, we ignore what the scan event has to say about this.
            if (!MappedByUser)
            {
                Wasmapped = e.WasMapped ?? false;
                CalcValues(Ody);
            }

            SurfaceGravity = Math.Round(SurfaceGravity / 10, 2);

            if (BodyName.StartsWith(SystemName, StringComparison.OrdinalIgnoreCase) && BodyName.Length > SystemName.Length)
            {
                BodyNameLocal = BodyName.Remove(0, SystemName.Length + 1).ToUpperInvariant();
            }
            else
            {
                BodyNameLocal = "";
            }

            UpdateStatus();
        }
        #endregion

        #region Body Info
        private StarType starType;
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

        private PlanetClass _planetClass;
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
            }
        }

        public double StellarMass { get; set; }
        public double MassEM { get; set; }

        private double _gravity;

        public double SurfaceGravity
        {
            get { return _gravity; }
            set { _gravity = value; OnPropertyChanged(); }
        }

        private int _mappedValue;
        public int MappedValue
        {
            get => _mappedValue;
            set { _mappedValue = value; OnPropertyChanged(); OnPropertyChanged("ScanValue"); }
        }

        private int _fssValue;
        public int FssValue
        {
            get { return _fssValue; }
            set { _fssValue = value; OnPropertyChanged(); OnPropertyChanged("ScanValue"); }
        }

        public int BonusValue;

        public int ScanValue
        {
            get
            {
                return _mappedByUser || IsStar ? _mappedValue : _fssValue;
            }
            set { }
        }

        public TerraformState TerraformState { get; set; }

        public string SystemName { get; set; }

        public long SystemAddress { get; set; }
        public string BodyName { get; set; }
        public string BodyNameLocal { get; set; }

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
            get { return !IsPlanet ? true : _wasMapped; }
            set { _wasMapped = value; OnPropertyChanged(); }
        }

        [IgnoreDataMember]
        public bool WorthMapping
        {
            get => MappedValue >= Settings.SettingsInstance.Value.WorthMappingValue &&
                    (Settings.SettingsInstance.Value.WorthMappingDistance <= 0 || Settings.SettingsInstance.Value.WorthMappingDistance >= DistanceFromArrivalLs) &&
                    MappedByUser == false;
            set { OnPropertyChanged(); }
        }
        [IgnoreDataMember]
        public bool IsKnownToEDSM { get => WasDiscovered; }

        private bool _mappedByUser;
        public bool MappedByUser { get => _mappedByUser; set { _mappedByUser = value; OnPropertyChanged(); OnPropertyChanged("ScanValue"); } }

        public bool EffeicentMapped;
        [IgnoreDataMember]
        public bool IsStar { get { return StarType != StarType.Unknown; } }
        [IgnoreDataMember]
        public bool IsPlanet { get { return PlanetClass != PlanetClass.Unknown; } }
        [IgnoreDataMember]
        public bool IsNonBody { get { return !IsStar && !IsPlanet; } }
        [IgnoreDataMember]
        public bool Terraformable { get { return TerraformState is TerraformState.Terraformable or TerraformState.Terraforming or TerraformState.Terraformed; } }

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
                SurfaceGravity = e.SurfaceGravity;
                Landable = e.Landable;
                WasDiscovered = e.WasDiscovered;
                //Whan a body is mapped by the user a scan event is fired
                //For some reason that scan event can report the body as not mapped when it has been previously
                //So, if the user has just mapped a body, we ignore what the scan event has to say about this.
                if (!MappedByUser)
                {
                    Wasmapped = e.Wasmapped;
                }
                BodyNameLocal = e.BodyNameLocal.ToUpperInvariant();

                CalcValues(Ody);
                UpdateStatus();
                SetBodyName();
            });
        }

        private void SetBodyName()
        {
            if (BodyName.StartsWith(SystemName, StringComparison.OrdinalIgnoreCase) && BodyName.Length > SystemName.Length)
            {
                BodyNameLocal = BodyName.Remove(0, SystemName.Length + 1).ToUpperInvariant();
                return;
            }

            BodyNameLocal = "";
        }

        internal void UpdateFromScan(ScanEvent.ScanEventArgs e, bool Ody)
        {
            SystemName = e.StarSystem;
            SystemAddress = e.SystemAddress;
            BodyName = e.BodyName;
            BodyID = e.BodyID;
            DistanceFromArrivalLs = (int)e.DistanceFromArrivalLs;
            StarType = e.StarType;
            StellarMass = e.StellarMass ?? 0;
            PlanetClass = e.PlanetClass;
            MassEM = e.MassEM ?? 0;
            TerraformState = e.TerraformState;
            SurfaceGravity = e.SurfaceGravity ?? 0;
            Landable = e.Landable ?? false;
            WasDiscovered = e.WasDiscovered ?? false;
            //Whan a body is mapped by the user a scan event is fired
            //For some reason that scan event can report the body as not mapped when it has been previously
            //So, if the user has just mapped a body, we ignore what the scan event has to say about this.
            if (!MappedByUser)
            {
                Wasmapped = e.WasMapped ?? false;
                CalcValues(Ody);
            }
            SurfaceGravity = Math.Round(SurfaceGravity / 10, 2);

            SetBodyName();
            UpdateStatus();
        }

        internal void UpdateFromEDSM(SystemBody e)
        {
            BodyID = e.BodyID;
            BodyName = e.BodyName;
            DistanceFromArrivalLs = e.DistanceFromArrivalLs;
            WasDiscovered = e.WasDiscovered;
            MappedValue = e.MappedValue;

            SetBodyName();
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
            if (WorthMapping && IsPlanet)
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
