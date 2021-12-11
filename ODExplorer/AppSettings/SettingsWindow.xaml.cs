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

        private readonly EstimatedScanValue scanValue;

        private readonly ScannedBioData scannedBioData;
        public SettingsWindow(MainWindow main)
        {
            AppSettings = main.AppSettings;
            scanValue = main.NavData.ScanValue;
            scannedBioData = main.NavData.ScannedBio;

            //Make a copy of the current settings
            AppSettings.CloneValues();

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ = AppSettings.SaveSettings();
        }

        private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            AppSettings.ClonedValues.Reset();
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
            AppSettings.SetClonedValues();
            DialogResult = true;
        }

        private void Cancel_Btn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
