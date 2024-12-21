using ODExplorer.ViewModels.ViewVMs;
using ODUtils.Commands;
using System.Windows.Forms;
using System.Windows.Input;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for CommanderSettingsControl.xaml
    /// </summary>
    public partial class CommanderSettingsControl : System.Windows.Controls.UserControl
    {
        public CommanderSettingsControl()
        {
            InitializeComponent();
            Loaded += CommanderSettingsControl_Loaded;  
        }

        private void CommanderSettingsControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel model)
            {
                ScanNewDirectory = new RelayCommand(OnScanNewDirectory, (_) => model.IsLoaded);
                ScanDirectoryBtn.Command = ScanNewDirectory;
            }
        }

        public ICommand? ScanNewDirectory { get; private set; }

        private void OnScanNewDirectory(object? obj)
        {
            if (DataContext is SettingsViewModel model)
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    model.OnScanNewDirectory(dialog.SelectedPath);
                }
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel model)
            {
                if (model.SelectedCommander is null)
                {
                    return;
                }

                var folderDialog = new FolderBrowserDialog
                {
                    InitialDirectory = model.SelectedCommander.JournalPath
                };

                var result = folderDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    model.OnSetNewDir(folderDialog.SelectedPath);
                }
            }
        }
    }
}
