using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ODExplorer.AboutBox
{
    /// <summary>
    /// Interaction logic for AboutBox.xaml
    /// </summary>
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ProcessStartInfo psi = new();
            psi.UseShellExecute = true;
            psi.FileName = e.Uri.AbsoluteUri;
            _ = Process.Start(psi);
            e.Handled = true;
        }

        private void PayPalDonateButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new();
            psi.UseShellExecute = true;
            psi.FileName = "https://www.paypal.com/donate/?business=UPEJS3PN7H4XJ&no_recurring=0&item_name=Creator+of+OD+Software.+Thank+you+for+your+donation.&currency_code=GBP";
            _ = Process.Start(psi);
            e.Handled = true;
        }
    }
}
