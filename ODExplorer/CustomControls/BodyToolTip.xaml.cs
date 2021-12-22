using ODExplorer.NavData;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace ODExplorer.CustomControls
{
    /// <summary>
    /// Interaction logic for BodyToolTip.xaml
    /// </summary>
    public partial class BodyToolTip : UserControl
    {
        #region Property Changed
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (Dispatcher.CheckAccess())
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                return;
            }

            Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
        #endregion

        private SystemBody body;
        public SystemBody Body { get => body; set { body = value; OnPropertyChanged(); } }
        public BodyToolTip(SystemBody body)
        {
            this.body = body;
            InitializeComponent();
        }
    }
}
