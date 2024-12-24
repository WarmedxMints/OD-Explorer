using EliteJournalReader;
using ODExplorer.Models;
using ODUtils.Dialogs.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using ODUtils.Models;
using System.Collections.ObjectModel;
using ODExplorer.Extensions;
using ODExplorer.Stores;
using ToolTip = System.Windows.Controls.ToolTip;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class StarSystemViewModel(StarSystem system, SettingsStore settingsStore, NotificationStore notificationStore) : OdViewModelBase
    {
        private readonly SettingsStore _settingsStore = settingsStore;
        private readonly NotificationStore notificationStore = notificationStore;
        private readonly StarSystem _system = system;
        public long Address => _system.Address;
        public string Name => _system.Name.ToUpperInvariant();
        public Position Position => _system.Position;
        public string Region => _system.Region.Name;
        public string StarClass => _system.StarType == StarType.Unknown ? "?" : _system.StarType.ToString();
        public StarType StarType => _system.StarType;
        public Jumponium GreenSystem => CheckSystemMaterials();
        public long EstValue
        {
            get
            {
                var knownValue = bodies.Sum(x => x.MappedValueActual);

                if (knownValue < _system.EstimatedValue)
                    return _system.EstimatedValue;
                return knownValue;
            }
        }
        public string EstimatedValue => EstValue == 0 ? "?" : EstValue.ToString("N0");

        public bool IsKnownToEDSM => _system.IsKnownToEDSM;
        public bool VisitedByCommander => _system.VisitedByCommander;
        public int DiscoveredBodiesCount => _system.DiscoveredBodyCount;
        public string EdsmPercentage
        {
            get
            {
                if (_system.BodyCount < 0 || _system.EdsmScannedBodyCount < 0)
                    return string.Empty;
                if (_system.BodyCount == 0)
                    return $"0 %";
                int percent = (int)Math.Round((double)(100 * _system.EdsmScannedBodyCount) / KnownBodyCount);
                //Clamp the percent just for the rare systems which have changed name (Such as Delphi) and give erroneous results from EDSM
                return $"{Math.Clamp(percent, 0, 100):N0} %";
            }
        }
        public int KnownBodyCount => _system.BodyCount >= 0 ? _system.BodyCount : 0;
        public string EdsmUrl => _system.EdsmUrl;
        public string JumpDistanceRemaining { get; set; } = string.Empty;
        public string JumpDistanceToSystem { get; set; } = string.Empty;
        public int BodyCount => Bodies.Where(x => x.IsNonBody == false && x.IsEdsmVb == false).Count();

        public DataState DataMode { get; set; }
        public int DataCount => DataMode switch
        {
            DataState.Sold => _system.SoldCount,
            DataState.Lost => _system.LostCount,
            _ => _system.UnsoldCount,
        };

        public long DataValue => DataMode switch
        {
            DataState.Sold => bodies.Where(x => x.DataState == DataState.Sold).Where(x => x.SoldDataValue > 0).Sum(x => x.SoldDataValue),
            DataState.Lost => bodies.Where(x => x.DataState == DataState.Lost).Where(x => x.LostDataValue > 0).Sum(x => x.LostDataValue),
            _ => bodies.Where(x => x.DataState == DataState.Unsold).Where(x => x.DataValue > 0).Sum(x => x.DataValue),
        };

        private ObservableCollection<SystemBodyViewModel> bodies = [];
        public ObservableCollection<SystemBodyViewModel> Bodies { get => bodies; set { bodies = value; OnPropertyChanged(nameof(Bodies)); } }

        public int PercentageScanned
        {
            get
            {
                if (_system.AllBodiesFound)
                {
                    return 100;
                }

                int scannedCount = Bodies.Count(x => x.IsNonBody == false && x.PlanetClass != PlanetClass.EdsmValuableBody);

                if (scannedCount <= 0)
                {
                    return 0;
                }

                int percent = (int)Math.Round((double)(100 * scannedCount) / DiscoveredBodiesCount);

                return percent < 0 ? 0 : percent;
            }
        }

        private ContextMenu? contextMenu;
        public ContextMenu ContextMenu
        {
            get
            {
                if (contextMenu is null)
                {
                    contextMenu = new();

                    MenuItem menuItem = new();
                    if (string.IsNullOrEmpty(EdsmUrl))
                    {
                        menuItem.Header = "System Not Known to EDSM";
                        menuItem.IsEnabled = false;
                    }
                    else
                    {
                        menuItem.Header = "Open System on EDSM";
                        menuItem.Click += OpenEDSMUrl;
                    }

                    _ = contextMenu.Items.Add(menuItem);

                    menuItem = new MenuItem
                    {
                        Header = $"Copy '{Name}' to Clipboard",

                    };
                    menuItem.Click += CopySystemNameToClipboard;

                    _ = contextMenu.Items.Add(menuItem);
                }

                return contextMenu;
            }
        }

        public ToolTip JumponiumToolTip
        {
            get
            {
                return GreenSystem switch
                {
                    Jumponium.Basic => new() { Content = "Jumponium - This system contains the materials required for a basic synth" },
                    Jumponium.Standard => new() { Content = "Jumponium - This system contains the materials required for a standard synth" },
                    Jumponium.Premium => new() { Content = "Jumponium - This system contains the materials required for a premium synth" },
                    _ => new() { Content = "Jumponium - This system does not contain the materials required for a synth" },
                };
            }
        }

        internal void OnAllBodiesFound()
        {
            OnPropertyChanged(nameof(BodyCount));
            OnPropertyChanged(nameof(KnownBodyCount));
            OnPropertyChanged(nameof(PercentageScanned));
        }

        public void OnSystemUpdatedFromEDSM()
        {
            foreach (var body in _system.SystemBodies)
            {
                bool knownBody = Bodies.FirstOrDefault(x => x.EdsmBodyId == body.EdsmBodyID) != default;

                if (knownBody)
                {
                    continue;
                }

                Bodies.AddToCollection(new(body, _settingsStore));
            }
            OnPropertyChanged(nameof(Bodies));
            OnPropertyChanged(nameof(EstimatedValue)); 
            OnPropertyChanged(nameof(IsKnownToEDSM));
            OnPropertyChanged(nameof(EdsmPercentage));
        }

        private void OpenEDSMUrl(object sender, RoutedEventArgs e)
        {
            ODUtils.Helpers.OperatingSystem.OpenUrl(EdsmUrl);
        }

        private void CopySystemNameToClipboard(object sender, RoutedEventArgs e)
        {
            notificationStore.CopyToClipBoard(Name);
        }

        internal SystemBodyViewModel UpdateBody(SystemBody e)
        {
            var known = Bodies.FirstOrDefault(x => x.BodyID == e.BodyID);

            if (known is not null)
            {
                known.AddOrganicItems();
                known.OnBodyUpdated();
                OnPropertyChanged(nameof(known));
                OnPropertyChanged(nameof(Bodies));
                OnPropertyChanged(nameof(PercentageScanned));
                OnPropertyChanged(nameof(GreenSystem));
                OnPropertyChanged(nameof(EstimatedValue));
                return known;
            }

            var newBody = new SystemBodyViewModel(e, _settingsStore);
            newBody.AddOrganicItems();
            Bodies.AddToCollection(newBody);
            OnPropertyChanged(nameof(Bodies));
            OnPropertyChanged(nameof(PercentageScanned));
            OnPropertyChanged(nameof(GreenSystem));
            OnPropertyChanged(nameof(EstimatedValue));
            return newBody;

        }

        internal static StarSystemViewModel BuildSystemForCartoDeteailsView(StarSystem system, SettingsStore settingsStore, NotificationStore notificationStore, DataState dataState)
        {
            var ret = new StarSystemViewModel(system, settingsStore, notificationStore) { DataMode = dataState };

            ret.Bodies.AddRangeToCollection(system.SystemBodies.Where(x => x.PlanetClass != PlanetClass.EdsmValuableBody && x.BodyDataState == dataState).Select(x => new SystemBodyViewModel((SystemBody)x, settingsStore)).OrderBy(x => x.BodyID));

            return ret;
        }

        private static readonly PlanetMaterial BasicJumponium = PlanetMaterial.carbon | PlanetMaterial.vanadium | PlanetMaterial.germanium;
        private static readonly PlanetMaterial StandardJumponium = BasicJumponium | PlanetMaterial.cadmium | PlanetMaterial.niobium;
        private static readonly PlanetMaterial PremiumJumponium = StandardJumponium | PlanetMaterial.yttrium | PlanetMaterial.polonium;

        private Jumponium CheckSystemMaterials()
        {
            PlanetMaterial mats = PlanetMaterial.None;

            foreach (var body in Bodies)
            {
                if (body.LandableBool == false || body.Materials is null || body.Materials.Count <= 0)
                    continue;

                //mats.AddRange(body.Materials.Where(x => mats.Contains(x.Name) == false).Select(x => x.Name));
                foreach (var material in body.Materials)
                {
                    mats |= material.Name;
                }
            }

            if (ODUtils.Helpers.EnumUtility.ContainsAllShipMaterials(mats, PremiumJumponium))
            {
                return Jumponium.Premium;
            }

            if (ODUtils.Helpers.EnumUtility.ContainsAllShipMaterials(mats, StandardJumponium))
            {
                return Jumponium.Standard;
            }

            if (ODUtils.Helpers.EnumUtility.ContainsAllShipMaterials(mats, BasicJumponium))
            {
                return Jumponium.Standard;
            }

            return Jumponium.None;
        }
    }
}
