using ODExplorer.NavData;
using ODExplorer.OrganicData;
using System.Windows;

namespace ODExplorer.AppSettings
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public Settings AppSettings { get; set; }

        public SettingsValues Values { get; set; }

        private readonly EstimatedScanValue scanValue;

        private readonly ScannedBioData scannedBioData;
        public SettingsWindow(MainWindow main)
        {
            AppSettings = main.AppSettings;
            scanValue = main.NavData.ScanValue;
            scannedBioData = main.NavData.ScannedBio;

            //Make a copy of the current settings
            Values = new();
            Values.Copy(AppSettings.Value);

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ = AppSettings.SaveSettings();
        }

        private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Values.Reset();
        }

        private void ResetExplorationValueButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to reset the estimated scan value?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                scanValue.Reset();
            }
        }

        private void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            //Save Values
            AppSettings.Value.Copy(Values);
            _ = AppSettings.SaveSettings();
            DialogResult = true;
            SystemCommands.CloseWindow(this);
        }

        private void Cancel_Btn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            SystemCommands.CloseWindow(this);
        }

        private void ClearBioLogicalData_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to reset the biological scan data?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                scannedBioData.ResetData();
            }
        }
    }
}
