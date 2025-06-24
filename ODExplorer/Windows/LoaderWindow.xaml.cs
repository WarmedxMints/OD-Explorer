using ODExplorer.ViewModels.ViewVMs;
using ODUtils.Dialogs;
using ODUtils.Models;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ODExplorer.Windows
{
    /// <summary>
    /// Interaction logic for LoaderWindow.xaml
    /// </summary>
    public partial class LoaderWindow : Window
    {
        public LoaderWindow(LoaderViewModel viewModel)
        {
            DataContext = viewModel;
            Title = $"OD Explorer {App.AppVersion}";
            InitializeComponent();
            Loaded += (sender, e) => _= Loader_Loaded(sender, e);
        }

        private async Task Loader_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoaderViewModel viewModel)
            {
                viewModel.OnUpdateComplete += OnUpdateCompete;
                viewModel.OnUpdateAvailable += (updateInfo) => _ = OnUpdateAvailable(updateInfo);
                await viewModel.Run().ConfigureAwait(true);
                return;
            }
            DialogResult = false;
            this.Close();
        }

        private async Task OnUpdateAvailable(UpdateInfo updateInfo)
        {
#if INSTALL
            var result = ODMessageBox.ShowUpdateWindow(this, "OD Explorer", $"Version {updateInfo.Version} is available.\nWould you like to download?", updateInfo.PatchNotes);

            if (result == MessageBoxResult.Yes)
            {
                var filename = Path.GetFileName(updateInfo.FileUrl);
                var tempPath = Path.GetTempPath();
                var filePath = Path.Combine(tempPath, filename);

                result = ODMessageBox.ShowDownloadBox(this, "OD Explorer", $"Downloading {filename}", updateInfo.FileUrl, filePath);
                if (result == MessageBoxResult.OK)
                {
                    Process.Start(filePath);
                    DialogResult = false;
                    Application.Current.Shutdown();
                    return;
                }

                result = ODMessageBox.ShowUpdateWindow(this, "OD Explorer", "Error downloading setup file.\nWould you like to go to the download page?", updateInfo.PatchNotes);
                if (result == MessageBoxResult.OK)
                {
                    ODUtils.Helpers.OperatingSystem.OpenUrl(updateInfo.Url);
                }
            }
#else
            var result = ODMessageBox.ShowUpdateWindow(this, "OD Explorer", $"Version {updateInfo.Version} is available.\nWould you like to go to the download page?", updateInfo.PatchNotes);

            if (result == MessageBoxResult.Yes)
            {
                ODUtils.Helpers.OperatingSystem.OpenUrl(updateInfo.Url);
            }
#endif
            if (DataContext is LoaderViewModel viewModel)
            {
                await viewModel.MigrateDatabase();
            }            
        }

        private void OnUpdateCompete()
        {
            Application.Current.Dispatcher.Invoke(() => DialogResult = true);
        }
    }
}
