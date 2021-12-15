using ODExplorer.Utils;
using System.Collections.Generic;

namespace ODExplorer.AppSettings
{
    public class DisplaySettings : PropertyChangeNotify
    {
        private List<DatagridLayout> _csbColumnOrder = new();

        public List<DatagridLayout> CSBColumnOrder
        {
            get
            {
                if (_csbColumnOrder == null)
                {
                    SetDefaultColumOrder();
                }

                return _csbColumnOrder;
            }
            set => _csbColumnOrder = value;
        }

        private bool _showCSBHeaders = true;
        public bool ShowCSBHeaders
        {
            get => _showCSBHeaders;
            set { _showCSBHeaders = value; OnPropertyChanged(); }
        }

        #region Column Visbility Bools
        private bool _showTypeColumn = true;
        public bool ShowTypeColumn
        {
            get => _showTypeColumn;
            set { _showTypeColumn = value; OnPropertyChanged(); }
        }

        private bool _showInfoColumn = true;
        public bool ShowInfoColumn
        {
            get => _showInfoColumn;
            set { _showInfoColumn = value; OnPropertyChanged(); }
        }

        private bool _showDistanceColumn = true;
        public bool ShowDistanceColumn
        {
            get => _showDistanceColumn;
            set { _showDistanceColumn = value; OnPropertyChanged(); }
        }

        private bool _showValueColumn = true;
        public bool ShowValueColumn
        {
            get => _showValueColumn;
            set { _showValueColumn = value; OnPropertyChanged(); }
        }
        #endregion

        #region Info Panel Vibility Bools
        private bool _showSurfaceTemp = true;
        public bool ShowSurfaceTemp
        {
            get => _showSurfaceTemp;
            set { _showSurfaceTemp = value; OnPropertyChanged(); }
        }

        private bool _showSurfacePressure = true;
        public bool ShowSurfacePressure
        {
            get => _showSurfacePressure;
            set { _showSurfacePressure = value; OnPropertyChanged(); }
        }

        private bool _showGeoSignals = true;
        public bool ShowGeoSignals
        {
            get => _showGeoSignals;
            set { _showGeoSignals = value; OnPropertyChanged(); }
        }

        private bool _showUnmapped = true;
        public bool ShowUnmapped
        {
            get => _showUnmapped;
            set { _showUnmapped = value; OnPropertyChanged(); }
        }

        private bool _showTerraformable = true;
        public bool ShowTerraformable
        {
            get => _showTerraformable;
            set { _showTerraformable = value; OnPropertyChanged(); }
        }

        private bool _showHasRings = true;
        public bool ShowHasRings
        {
            get => _showHasRings;
            set { _showHasRings = value; OnPropertyChanged(); }
        }
        private bool _showBioSignals = true;
        public bool ShowBioSignals
        {
            get => _showBioSignals;
            set { _showBioSignals = value; OnPropertyChanged(); }
        }

        private bool _showSurfaceGravity = true;
        public bool ShowSurfaceGravity
        {
            get => _showSurfaceGravity;
            set { _showSurfaceGravity = value; OnPropertyChanged(); }
        }
        #endregion

        #region Systems in Route Bools
        private bool showJumpDistRemaining = true;
        public bool ShowJumpDistRemaining
        {
            get => showJumpDistRemaining;
            set { showJumpDistRemaining = value; OnPropertyChanged(); }
        }

        private bool showJumpDistToSystem = true;
        public bool ShowJumpDistToSystem
        {
            get => showJumpDistToSystem;
            set { showJumpDistToSystem = value; OnPropertyChanged(); }
        }

        private bool showVaulableBodies = true;
        public bool ShowVaulableBodies
        {
            get => showVaulableBodies;
            set { showVaulableBodies = value; OnPropertyChanged(); }
        }
        #endregion

        private void SetDefaultColumOrder()
        {
            _csbColumnOrder.Clear();

            _csbColumnOrder.Add(new DatagridLayout() { Header = "NAME", DisplayIndex = 0 });
            _csbColumnOrder.Add(new DatagridLayout() { Header = "TYPE", DisplayIndex = 1 });
            _csbColumnOrder.Add(new DatagridLayout() { Header = "INFO", DisplayIndex = 2 });
            _csbColumnOrder.Add(new DatagridLayout() { Header = "DISTANCE", DisplayIndex = 3 });
            _csbColumnOrder.Add(new DatagridLayout() { Header = "VALUE", DisplayIndex = 4 });
        }
    }

    public struct DatagridLayout
    {
        public string Header { get; set; }
        public int DisplayIndex { get; set; }
    }
}
