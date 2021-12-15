using EliteJournalReader;
using EliteJournalReader.Events;
using ODExplorer.Utils;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.NavData
{

    public class SystemInfo : PropertyChangeNotify
    {
        #region Constructors
        public SystemInfo() { }

        public SystemInfo(Route route)
        {
            StarClass = route.StarClass.ToUpperInvariant();
            SystemName = route.StarSystem.ToUpperInvariant();
            SystemAddress = route.SystemAddress;
            SystemPos = route.StarPos.Copy();
        }

        public SystemInfo(SystemInfo sys)
        {
            starClass = sys.StarClass;
            systemName = sys.SystemName;
            IsScoopable = sys.IsScoopable;
            isKnownToEDSM = sys.IsKnownToEDSM;
            sysValue = sys.SysValue;
            SystemAddress = sys.SystemAddress;
        }

        public SystemInfo(LocationEvent.LocationEventArgs e)
        {
            systemName = e.StarSystem.ToUpperInvariant();
            SystemAddress = e.SystemAddress;
        }
        #endregion

        #region Properties
        public SystemPosition SystemPos { get; set; }

        private double jumpDistanceToSystem;

        public double JumpDistanceToSystem
        {
            get => jumpDistanceToSystem;
            set
            {
                jumpDistanceToSystem = value;
                OnPropertyChanged();
            }
        }


        private double jumpDistanceRemaining;

        public double JumpDistanceRemaining
        {
            get => jumpDistanceRemaining;
            set
            {
                jumpDistanceRemaining = value;
                OnPropertyChanged();
            }
        }

        private string starClass;

        public string StarClass
        {
            get => starClass;
            set
            {
                starClass = value;
                OnPropertyChanged();
                OnPropertyChanged("IsScoopable");
            }
        }

        private string systemName = "Unkown";
        public string SystemName
        {
            get => systemName;
            set
            {
                systemName = value;
                OnPropertyChanged();
            }
        }

        public long SystemAddress { get; set; }

        private long sysValue = -1;

        public long SysValue
        {
            get => sysValue;
            set { sysValue = value; OnPropertyChanged("SystemValue"); }
        }

        [IgnoreDataMember]
        public string SystemValue
        {
            get => SysValue < 0 ? "?" : $"{SysValue:N0}";
            set => OnPropertyChanged();
        }

        [IgnoreDataMember]
        public bool IsScoopable
        {
            get => StarClass switch
            {
                "K" or "G" or "B" or "F" or "O" or "A" or "M" => true,
                _ => false,
            };
            set => OnPropertyChanged();
        }

        private bool isKnownToEDSM;

        public bool IsKnownToEDSM
        {
            get => isKnownToEDSM;
            set
            {
                isKnownToEDSM = value;
                OnPropertyChanged();
            }
        }
        [IgnoreDataMember]
        public bool PolledEDSMValue { get; set; }

        private int _discoveredBodiesCount;

        public int DiscoveredBodiesCount
        {
            get => _discoveredBodiesCount;
            set
            {
                _discoveredBodiesCount = value;
                OnPropertyChanged();
            }
        }

        private bool _allBodiesFound;

        public bool AllBodiesFound
        {
            get => _allBodiesFound;
            set
            {
                _allBodiesFound = value;
                PercentageScanned = 1;
            }
        }

        private int _knownBodies;

        public int KnownBodies
        {
            get => _knownBodies;
            set
            {
                _knownBodies = value;
                OnPropertyChanged();
            }
        }
        //Percentage of system scanned by body count
        //TODO See if I can refine this to operate by mass just as the in game scanner does
        [IgnoreDataMember]
        public int PercentageScanned
        {
            get
            {
                if (AllBodiesFound)
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
            set => OnPropertyChanged();
        }

        public Uri EDSMUrl { get; set; }

        //Context Menu for the datagrid
        private ContextMenu contextMenu;
        [IgnoreDataMember]
        public ContextMenu ContextMenu
        {
            get
            {
                if (contextMenu is null)
                {
                    contextMenu = new();

                    var menuStyle = Application.Current.FindResource("ContextMenuItem") as Style;

                    MenuItem menuItem = new();
                    menuItem.Style = menuStyle;
                    if (EDSMUrl == null)
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
                        Header = $"Copy '{SystemName}' to Clipboard",
                    };
                    menuItem.Style = menuStyle;
                    menuItem.Click += CopySystemNameToClipboard;

                    _ = contextMenu.Items.Add(menuItem);
                }

                return contextMenu;
            }
        }

        public ObservableCollection<SystemBody> Bodies { get; set; } = new();
        #endregion

        //This method is only ever called when valuable bodies are found on EDSM during the system information lookup
        public void AddBody(SystemBody bodyToAdd, bool fromDetailedScan, bool fromEDSM, bool Ody)
        {
            SystemBody bodyknown = Bodies.FirstOrDefault(x => x.BodyName == bodyToAdd.BodyName);

            if (bodyknown == default)
            {
                Bodies.AddToCollection(bodyToAdd);
                return;
            }

            if (fromDetailedScan)
            {
                bodyknown.UpdateFromDetailedScan(bodyToAdd, Ody);
                return;
            }

            if (fromEDSM)
            {
                bodyknown.UpdateFromEDSM(bodyToAdd);
                return;
            }
        }
        //Opens the system url on EDSM
        private void OpenEDSMUrl(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new();
            psi.UseShellExecute = true;
            psi.FileName = EDSMUrl.AbsoluteUri;
            _ = Process.Start(psi);
        }
        //Copies the system name to clipboard
        private void CopySystemNameToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(SystemName);
        }

        public override string ToString()
        {
            return systemName;
        }
    }
}
