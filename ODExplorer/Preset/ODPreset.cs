using ODExplorer.Utils;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace ODExplorer.Preset
{
    public class ODPreset : PropertyChangeNotify
    {
        private ObservableCollection<MenuItem> menuItems = new();
        [IgnoreDataMember]
        public ObservableCollection<MenuItem> MenuItems { get => menuItems; set { menuItems = value; OnPropertyChanged(); } }

        private ObservableCollection<ODPresetGroup> odPresetGroups = new();
        [IgnoreDataMember]
        public ObservableCollection<ODPresetGroup> ODPresetGroups { get => odPresetGroups; set { odPresetGroups = value; OnPropertyChanged(); } }
    }
}