using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.CustomControls
{
    /// <summary>
    /// Interaction logic for PresetSelectionButton.xaml
    /// </summary>
    public partial class MenuItemSelectButton : Button
    {
        public MenuItemSelectButton()
        {
            InitializeComponent();
        }

        private void Root_Loaded(object sender, RoutedEventArgs e)
        {
            menu.ItemsSource = MenuItems;
        }

        public ObservableCollection<MenuItem> MenuItems
        {
            get => (ObservableCollection<MenuItem>)GetValue(PresetCollectionGroup);
            set => SetValue(PresetCollectionGroup, value);
        }

        public static readonly DependencyProperty PresetCollectionGroup =
            DependencyProperty.Register("MenuItems", typeof(ObservableCollection<MenuItem>), typeof(MenuItemSelectButton), new UIPropertyMetadata());

        public object Header
        {
            get => GetValue(StringProperty);
            set => SetValue(StringProperty, value);
        }

        public static readonly DependencyProperty StringProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(MenuItemSelectButton), new UIPropertyMetadata());

        /// <summary>Handle the button click event. Open the context menu if it was not already open.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // IsOpen will always be false here, but IsVisible will give us the menu state at the time the button was clicked.
            if (!menu.IsVisible)
            {
                menu.PlacementTarget = (Button)sender;
                menu.VerticalOffset = 5;
                menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                //menu.Width = ((Button)sender).ActualWidth + 5;
                menu.IsOpen = true;
            }
        }

        //Disable right click to open context menu
        private void Root_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Menu_Closed(object sender, RoutedEventArgs e)
        {

        }


    }
}
