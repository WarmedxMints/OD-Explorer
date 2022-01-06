using ODExplorer.Utils;
using System.Collections.ObjectModel;

namespace ODExplorer.Preset
{
    public class ODPresetSubMenuItem : PropertyChangeNotify
    {
        private string menuTitle;
        public string MenuTitle { get => menuTitle; set { menuTitle = value; OnPropertyChanged(); } }

        private ObservableCollection<object> menuItems = new();
        public ObservableCollection<object> MenuItems { get => menuItems; set { menuItems = value; OnPropertyChanged(); } }
    }
}