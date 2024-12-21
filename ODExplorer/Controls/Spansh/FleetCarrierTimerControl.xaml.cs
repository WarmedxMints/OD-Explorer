using ODExplorer.ViewModels.ViewVMs;
using System.Windows;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for FleetCarrierTimerControl.xaml
    /// </summary>
    public partial class FleetCarrierTimerControl : UserControl
    {
        public FleetCarrierTimerControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SpanshViewModel spanshViewModel)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "Browse sound files",

                    CheckFileExists = true,
                    CheckPathExists = true,

                    DefaultExt = "csv",
                    Filter = "Sound files (*.wav, *.mp3)|*.wav;*.mp3",
                    FilterIndex = 2,
                    RestoreDirectory = true,

                    ReadOnlyChecked = true,
                    ShowReadOnly = true
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    spanshViewModel.SetCustomFile(openFileDialog.FileName);
                }
            }
        }
    }
}
