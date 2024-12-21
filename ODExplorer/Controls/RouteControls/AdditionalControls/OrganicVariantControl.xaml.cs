using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for OrganicVariantControl.xaml
    /// </summary>
    public partial class OrganicVariantControl : UserControl
    {
        public OrganicVariantControl()
        {
            InitializeComponent();
        }

        private void ListBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (sender is not Control control)
            {
                return;
            }
            e.Handled = true;
            var wheelArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = MouseWheelEvent,
                Source = control
            };
            var parent = control.Parent as UIElement;
            parent?.RaiseEvent(wheelArgs);
        }
    }
}
