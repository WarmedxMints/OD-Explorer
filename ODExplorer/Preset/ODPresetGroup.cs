using ODExplorer.Utils;
using System.Collections.ObjectModel;

namespace ODExplorer.Preset
{
    public class ODPresetGroup : PropertyChangeNotify
    {
        private string groupName;
        public string GroupName { get => groupName; set { groupName = value;  OnPropertyChanged(); } }

        private ObservableCollection<object> menuItems = new();
        public ObservableCollection<object> MenuItems { get => menuItems; set { menuItems = value; OnPropertyChanged(); } }
    }
}
