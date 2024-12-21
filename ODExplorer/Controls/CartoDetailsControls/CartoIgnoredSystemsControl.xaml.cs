using ODExplorer.ViewModels.ModelVMs;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.Controls
{
    /// <summary>
    /// Interaction logic for CartoIgnoredSystemsControl.xaml
    /// </summary>
    public partial class CartoIgnoredSystemsControl : UserControl, INotifyPropertyChanged
    {
        #region Property Changed
        // Declare the event
        public event PropertyChangedEventHandler? PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            if (Application.Current is null)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
                catch (Exception e) { Console.WriteLine(e.Message); }
            });
        }
        #endregion

        private IgnoredSystemsViewModel? selectedSystem;
        public IgnoredSystemsViewModel? SelectedSystem
        {
            get => selectedSystem;
            set
            {
                selectedSystem = value;
                OnPropertyChanged(nameof(SelectedSystem));
            }
        }

        public CartoIgnoredSystemsControl()
        {
            InitializeComponent();
        }
    }
}
