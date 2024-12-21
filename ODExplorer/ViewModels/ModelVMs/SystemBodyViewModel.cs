using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.Controls;
using ODExplorer.Extensions;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Extensions;
using ODUtils.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class SystemBodyViewModel(SystemBody systemBody, SettingsStore settingsStore) : OdViewModelBase
    {
        private SettingsStore SettingsStore => settingsStore;
        private readonly SystemBody _body = systemBody;
        public SystemBody Body => _body;
        public string GoverningStar => _body.GoverningStar.ToString();
        #region DataGrid Properties
        public string Name => BodyNameLocal().ToUpperInvariant();
        public string ScanStage => _body.ScanState.GetEnumDescription();
        public string BodyDescription
        {
            get
            {
                if (IsPlanet)
                    return _body.PlanetClass.GetDescription().ToUpperInvariant();
                if (IsStar)
                    return _body.StarType.GetDescription().ToUpperInvariant();
                return string.Empty;
            }
        }
        public string SurfaceTemp
        {
            get
            {
                if (IsEdsmVb || IsNonBody)
                    return string.Empty;

                return SettingsStore.SystemGridSetting.TemperatureDisplay switch
                {
                    Temperature.Celsius => $"{_body.SurfaceTemp - 273.15:N0} °C",
                    Temperature.Fahrenheit => $"{(_body.SurfaceTemp - 273.15) * 9 / 5 + 32:N0} °F",
                    _ => $"{_body.SurfaceTemp:N0} k",
                };
            }
        }

        public string SurfacePressure
        {
            get
            {
                if (_body.IsStar)
                    return $"{_body.Age_MY:N0} MY";

                if (IsEdsmVb || IsNonBody)
                    return string.Empty;

                if (HasAtmosphere)
                {
                    switch (SettingsStore.SystemGridSetting.PressureUnit)
                    {
                        case Pressure.Atmosphere:
                            var atm = _body.SurfacePressure / 101325;
                            if (atm >= 1000)
                                return $"{atm:N0} atm";
                            if (atm >= 10)
                                return $"{atm:N2} atm";
                            if (atm >= 1)
                                return $"{atm:N3} atm";
                            if(atm >= 0.01)
                                return $"{atm:N4} atm";
                            return $"{atm:N6} atm";
                        case Pressure.Psi:
                            return $"{_body.SurfacePressure / 6894.7572931783:N2} psi";
                        default:
                        case Pressure.Pascal:
                            return $"{_body.SurfacePressure:N0} pa";
                    }
                }
                return string.Empty;
            }
        }

        public string AtmosphereDescription => _body.Atmosphere.GetEnumDescription();

        public string AtmosphereFormula
        {
            get
            {
                if (_body.IsStar)
                    return $"{_body.StellarMass:N3} SM";
                if (!HasAtmosphere)
                    return string.Empty;
                string ret = _body.AtmosphereType switch
                {
                    AtmosphereClass.Unknown => "?",
                    AtmosphereClass.None => "",
                    AtmosphereClass.NoAtmosphere => "",
                    AtmosphereClass.SuitableForWaterBasedLife => "SWBL",
                    AtmosphereClass.AmmoniaOxygen => "NH\u2083 O\u2082",
                    AtmosphereClass.Ammonia => "NH\u2083",
                    AtmosphereClass.EarthLike => "N₂O₂",
                    AtmosphereClass.Water => "H\u2082O",
                    AtmosphereClass.CarbonDioxide => "CO\u2082",
                    AtmosphereClass.SulphurDioxide => "SO\u2082",
                    AtmosphereClass.Nitrogen => "N\u2082",
                    AtmosphereClass.WaterRich => "H\u2082O Rich",
                    AtmosphereClass.MethaneRich => "CH\u2084 Rich",
                    AtmosphereClass.AmmoniaRich => "NH\u2083 Rich",
                    AtmosphereClass.CarbonDioxideRich => "CO\u2082 Rich",
                    AtmosphereClass.Methane => "CH\u2084",
                    AtmosphereClass.Helium => "He",
                    AtmosphereClass.SilicateVapour => "Sil Vap",
                    AtmosphereClass.MetallicVapour => "Met Vap",
                    AtmosphereClass.NeonRich => "Ne Rich",
                    AtmosphereClass.ArgonRich => "Ar Rich",
                    AtmosphereClass.Neon => "Ne",
                    AtmosphereClass.Argon => "Ar",
                    AtmosphereClass.Oxygen => "O\u2082",
                    _ => "?",
                };
                return ret;
            }
        }
        public string MappedValue => _body.MappedValue.ToString("N0");
        public string FssValue => _body.FssValue.ToString("N0");
        public string DistanceFromArrivalLs => $"{_body.DistanceFromArrivalLs:N0} ls";
        public string OrbitalPeriod
        {
            get
            {
                if (_body.OrbitalPeriod > 1)
                    return $"{_body.OrbitalPeriod:N1} d ";
                return $"{_body.OrbitalPeriod * 24:N1} h ";
            }
        }
        public string RotationPeriod
        {
            get
            {
                if (_body.RotationPeriod > 1)
                    return $"{_body.RotationPeriod:N1} d ";
                return $"{_body.RotationPeriod * 24:N1} h ";
            }
        }
        public string WasMappedString => WasMapped.ToYesNo();
        public string WasDiscoveredString => WasDiscovered.ToYesNo();
        public bool HasRings => _body.Rings?.Count > 0;
        public string Radius
        {
            get => SettingsStore.SystemGridSetting.DistanceUnit switch
            {
                Distance.Miles => $"{_body.Radius * 0.62137:N0} mi",
                _ => $"{_body.Radius:N0} km"
            };
        }
        public ToolTip ToolTip
        {
            get
            {
                if (_body.PlanetClass == PlanetClass.EdsmValuableBody)
                {
                    return new()
                    {
                        Content = "Scan for more info"
                    };
                }

                if (IsNonBody)
                {
                    return new()
                    {
                        Content = "Non Body"
                    };
                }
                if (IsPlanet)
                {
                    return new()
                    {
                        SnapsToDevicePixels = true,
                        Content = new PlanetToolTipControl(this)
                    };
                }
                if (IsStar)
                {
                    return new()
                    {
                        SnapsToDevicePixels = true,
                        Content = new StarToolTipControl(this)
                    };
                }
                return new();
            }
        }
        public PlanetImage PlanetImage
        {
            get
            {
                return PlanetClass switch
                {
                    PlanetClass.MetalRichBody or PlanetClass.HighMetalContentBody or PlanetClass.RockyBody or PlanetClass.IcyBody or PlanetClass.RockyIceBody => PlanetImage.Planet,
                    PlanetClass.EarthlikeBody => PlanetImage.Earth,
                    PlanetClass.WaterWorld or PlanetClass.WaterGiant or PlanetClass.WaterGiantWithLife => PlanetImage.Water,
                    PlanetClass.AmmoniaWorld or PlanetClass.GasGiantWithWaterBasedLife or PlanetClass.GasGiantWithAmmoniaBasedLife or PlanetClass.SudarskyClassIGasGiant or PlanetClass.SudarskyClassIIGasGiant or PlanetClass.SudarskyClassIIIGasGiant or PlanetClass.SudarskyClassIVGasGiant or PlanetClass.SudarskyClassVGasGiant or PlanetClass.HeliumRichGasGiant or PlanetClass.HeliumGasGiant => PlanetImage.Gas,
                    PlanetClass.EdsmValuableBody or PlanetClass.Unknown => PlanetImage.Default,
                    _ => PlanetImage.None,
                };
            }
        }

        public string OrganicValues
        {
            get
            {
                if (OrganicScanItems == null || OrganicScanItems.Count == 0)
                    return string.Empty;

                if (_body.MinExoValue == _body.MaxExoValue)
                {
                    return $"{_body.MaxExoValue.FormatNumber()}";
                }
                return $"{_body.MinExoValue.FormatNumber()} - {_body.MaxExoValue.FormatNumber()}";
            }
        }

        public bool IsHighValueExo => _body.MinExoValue > settingsStore.SystemGridSetting.ExoValuableBodyValue;
        #endregion

        #region Shared Properties
        public DataState DataState => _body.BodyDataState;
        public long DataValue => _body.UnsoldCommanderValue;
        public long SoldDataValue => _body.SoldCommanderValue;
        public long LostDataValue => _body.LostCommanderValue;
        private string SystemName => _body.Owner.Name;
        public long BodyID => IsEdsmVb ? 0 : _body.BodyID;
        public string FullName => _body.BodyName;
        public DiscoveryStatus Status => _body.Status;
        public bool WorthMapping => Status == DiscoveryStatus.WorthMapping;
        public double DistanceFromArrival => _body.DistanceFromArrivalLs;
        public long MappedValueActual => _body.MappedValue;
        public bool WasMapped
        {
            get
            {
                if (IsStar || IsNonBody || IsEdsmVb)
                    return true;
                return _body.WasMapped;
            }
        }
        public bool WasDiscovered => (IsNonBody || IsEdsmVb) || _body.WasDiscovered;
        #endregion

        #region Star Properties
        public bool IsStar => _body.IsStar;
        public string SolarMasses => _body.StellarMass?.ToString("N3") ?? "1";
        public string StarLuminosity => _body.StarLuminosity.GetDescription();
        public string Age_MY => _body.Age_MY.ToString("N0");
        public string StarTypeDescription => _body.StarType.GetEnumDescription();
        #endregion

        #region Planet Properties
        public bool IsPlanet => _body.IsPlanet;
        public bool IsEdsmVb => _body.PlanetClass == PlanetClass.EdsmValuableBody;
        public PlanetClass PlanetClass => _body.PlanetClass;
        public string PlanetClassDescription => _body.PlanetClass.GetEnumDescription();
        public string EarthMasses => $"{_body.MassEM}";
        public string Gravity => $"{_body.SurfaceGravity:N2}";
        public string AtmosphereType => _body.AtmosphereType.GetDescription();
        public bool HasAtmosphere => _body.AtmosphereType is not AtmosphereClass.None and not AtmosphereClass.NoAtmosphere and not AtmosphereClass.Unknown;
        public string Volcanism => _body.Volcanism.GetEnumDescription();
        public string TidalLock => _body.TidalLock.ToYesNo();
        public string AxialTilt => $"{_body.AxialTilt:N3}°";
        public string AbsoluteMagnitude => $"{_body.AbsoluteMagnitude:N4}";
        public string TerraformStatus => _body.Terraformable.ToYesNo();
        public string Landable => _body.Landable.ToYesNo();
        public bool LandableBool => _body.Landable;
        public bool Terraformable => _body.Terraformable;
        public int GeologicalSignals => _body.GeologicalSignals;
        public string GeologicalSignalsString => _body.GeologicalSignals > 0 ? _body.GeologicalSignals.ToString() : string.Empty;
        public int BiologicalSignals => _body.BiologicalSignals;
        public string BiologicalSignalsString => _body.BiologicalSignals > 0 ? _body.BiologicalSignals.ToString() : string.Empty;
        public double SurfaceGravity => _body.SurfaceGravity;
        public List<PlanetRing>? Rings => _body.Rings;
        public List<PlanetRingViewModel> RingsView => _body.Rings is null ? [] : _body.Rings.Select(x => new PlanetRingViewModel(x, Body.BodyName)).ToList();
        public List<ScanItemComponent>? AtmosphereComposition => _body.AtmosphereComposition;
        public List<ShipMaterials>? Materials => _body.Materials;
        public Composition Composition => _body.Composition;
        public bool HasMaterials => Materials?.Count > 0;
        private ObservableCollection<OrganicScanItemViewModel> _organicScanItems = [];
        public ObservableCollection<OrganicScanItemViewModel> OrganicScanItems { get => _organicScanItems; set { _organicScanItems = value; OnPropertyChanged(nameof(OrganicScanItems)); } }
        #endregion
        public bool IsNonBody => _body.IsPlanet == false && _body.IsStar == false;

        #region Methods
        internal bool AddOrganicItems()
        {
            if (_body.OrganicScanItems is null || _body.OrganicScanItems.IsEmpty)
                return _body.BiologicalSignals > 0;

            var organicsToAdd = new List<OrganicScanItemViewModel>();

            foreach (var item in _body.OrganicScanItems)
            {
                var known = OrganicScanItems.FirstOrDefault(x => x.Item == item);

                if (known != null)
                {
                    known.OnInfoUpdated();
                    continue;
                }
                var newOrganic = new OrganicScanItemViewModel(item);

                foreach (var variant in item.Variants)
                {
                    newOrganic.Variants.Add(new(variant, newOrganic));
                }
                organicsToAdd.Add(newOrganic);
            }
            var biosToAdd = organicsToAdd.OrderBy(x => x.GenusEnglish).ThenBy(x => x.SpeciesEnglish);

            SetAlternationIndexes(settingsStore, biosToAdd);
            OrganicScanItems.AddRangeToCollection(biosToAdd);
            OnPropertyChanged(nameof(OrganicValues));
            OnPropertyChanged(nameof(OrganicScanItems));
            return true;
        }

        public void SetAlternationIndexes()
        {
            SetAlternationIndexes(settingsStore, OrganicScanItems);
        }

        private static void SetAlternationIndexes(SettingsStore settingsStore, IEnumerable<OrganicScanItemViewModel> biosToAdd)
        {
            var currentIndex = 1;
            OrganicScanItemViewModel? lastItem = null;
            foreach (var item in biosToAdd)
            {
                if (item.UnConfirmed && settingsStore.SystemGridSetting.FilterUnconfirmedBios)
                {
                    lastItem = item;
                    continue;
                }
                if (lastItem != null && lastItem.GenusCodex == item.GenusCodex)
                {
                    if (lastItem.UnConfirmed && settingsStore.SystemGridSetting.FilterUnconfirmedBios)
                    {
                        currentIndex = currentIndex == 0 ? 1 : 0;
                    }
                    item.AlternationIndex = currentIndex;
                    lastItem = item;
                    continue;

                }
                currentIndex = currentIndex == 0 ? 1 : 0;
                item.AlternationIndex = currentIndex;
                lastItem = item;
            }
        }

        internal void UpdateOrganicInfo()
        {
            if (OrganicScanItems is null || OrganicScanItems.Count == 0)
                return;

            SetAlternationIndexes();
            foreach (var item in OrganicScanItems)
            {
                item.OnInfoUpdated();
            }            
            OnPropertyChanged(nameof(OrganicValues));
            OnPropertyChanged(nameof(OrganicScanItems));
        }
        public string BodyNameLocal()
        {
            if (string.IsNullOrEmpty(SystemName))
            {
                return _body.BodyName;
            }

            return _body.BodyName.StartsWith(SystemName, StringComparison.OrdinalIgnoreCase) && _body.BodyName.Length > SystemName.Length
                ? _body.BodyName.Remove(0, SystemName.Length + 1)
                : _body.BodyName;
        }

        internal void OnBodyUpdated()
        {
            OnPropertyChanged(nameof(PlanetImage));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(WorthMapping));
            OnPropertyChanged(nameof(IsNonBody));
            OnPropertyChanged(nameof(IsStar));
            OnPropertyChanged(nameof(IsPlanet));
            OnPropertyChanged(nameof(BodyDescription));
            OnPropertyChanged(nameof(PlanetClass));
            OnPropertyChanged(nameof(DistanceFromArrivalLs));
            OnPropertyChanged(nameof(MappedValue));
            OnPropertyChanged(nameof(OrbitalPeriod));
            OnPropertyChanged(nameof(RotationPeriod));
            OnPropertyChanged(nameof(Radius));
            OnPropertyChanged(nameof(EarthMasses));
            OnPropertyChanged(nameof(AtmosphereType));
            OnPropertyChanged(nameof(AtmosphereFormula));
            OnPropertyChanged(nameof(HasAtmosphere));
            OnPropertyChanged(nameof(SurfaceTemp));
            OnPropertyChanged(nameof(SurfacePressure));
            OnPropertyChanged(nameof(SolarMasses));
            OnPropertyChanged(nameof(TidalLock));
            OnPropertyChanged(nameof(AxialTilt));
            OnPropertyChanged(nameof(StarLuminosity));
            OnPropertyChanged(nameof(AbsoluteMagnitude));
            OnPropertyChanged(nameof(Age_MY));
            OnPropertyChanged(nameof(TerraformStatus));
            OnPropertyChanged(nameof(Landable));
            OnPropertyChanged(nameof(LandableBool));
            OnPropertyChanged(nameof(WasMapped));
            OnPropertyChanged(nameof(WasDiscovered));
            OnPropertyChanged(nameof(Terraformable));
            OnPropertyChanged(nameof(GeologicalSignals));
            OnPropertyChanged(nameof(BiologicalSignals));
            OnPropertyChanged(nameof(SurfaceGravity));
            OnPropertyChanged(nameof(HasRings));
            OnPropertyChanged(nameof(OrganicScanItems));
            OnPropertyChanged(nameof(GoverningStar));
            OnPropertyChanged(nameof(IsHighValueExo));
            OnPropertyChanged(nameof(DistanceFromArrival));
        }
        #endregion
    }
}
