using ODExplorer.CustomMessageBox;
using ODExplorer.GeologicalData;
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

        private readonly ScannedGeoData scannedGeoData;

        public SettingsWindow(MainWindow main)
        {
            AppSettings = main.AppSettings;
            scanValue = main.NavData.ScanValue;
            scannedBioData = main.NavData.ScannedBio;
            scannedGeoData = main.NavData.ScannedGeo;
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
            MessageBoxResult result = ODMessageBox.Show(this,
                                                        "Do you want to reset the estimated scan value?",
                                                        MessageBoxButton.YesNo);

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
            MessageBoxResult result = ODMessageBox.Show(this,
                                                        "Do you want to reset the Biological scan data?",
                                                        MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                scannedBioData.ResetData();
            }
        }

        private void ClearGeoLogicalData_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = ODMessageBox.Show(this,
                                            "Do you want to reset the Geoological scan data?",
                                            MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                scannedGeoData.ResetData();
            }
        }
    }
}
