using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ODExplorer.Models
{
    public partial class PropertyChangeNotify : INotifyPropertyChanged
    {
        // Declare the event
        public event PropertyChangedEventHandler? PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
