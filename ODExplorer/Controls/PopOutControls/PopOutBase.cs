using ODExplorer.Models;
using ODExplorer.ViewModels.ModelVMs;
using ODExplorer.ViewModels.ViewVMs;
using ODUtils.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODExplorer.Controls
{
    public partial class PopOutBase : UserControl, INotifyPropertyChanged
    {
        #region Property Change
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
                catch (Exception) { }
            });
        }
        #endregion

        private bool forceClose;
        public event EventHandler<PopOutMode>? OnPopOutModeReset;

        public ICommand SetZoom { get; }
        public PopOutBase()
        {
            SetZoom = new RelayCommand<double>(OnSetZoom, (value) => ZoomLevel != value);
            Unloaded += PopOutBase_Unloaded;
        }

        private void OnSetZoom(double value)
        {
            ZoomLevel = value;
        }

        private void PopOutBase_Unloaded(object sender, RoutedEventArgs e)
        {
            if (forceClose == false && DataContext is MainViewModel viewModel)
            {
                viewModel.OnPopOutClose(this);
            }
        }

        private double zoomLevel = 1d;
        public double ZoomLevel { get => zoomLevel; set { zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); } }
        public virtual string Title { get; protected set; } = string.Empty;
        public virtual string WindowTitle => Count > 1 ? $"{Title} {Count}" : Title;
        public virtual WindowPositionViewModel Position { get; set; } = new();

        private PopOutMode mode;
        public PopOutMode Mode
        {
            get => mode;
            set
            {
                mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }

        private bool alwaysOnTop = true;
        public bool AlwaysOnTop { get => alwaysOnTop; set { alwaysOnTop = value; OnPropertyChanged(nameof(AlwaysOnTop)); } }

        private bool showTitle = true;
        public bool ShowTitle { get => showTitle; set { showTitle = value; OnPropertyChanged(nameof(ShowTitle)); } }

        private bool showInTaskBar = true;


        public bool ShowInTaskBar { get => showInTaskBar; set { showInTaskBar = value; OnPropertyChanged(nameof(ShowInTaskBar)); } }
        public int Count { get; protected set; }
        public virtual object? AdditionalSettings { get; protected set; }
        public virtual void ApplyStyles(PopOutMode mode, bool mouseEnter) { }

        public virtual void ApplyParams(PopOutParams popOutParams)
        {
            Position = popOutParams.Position.Clone();
            Mode = popOutParams.Mode;
            AlwaysOnTop = popOutParams.AlwaysOnTop;
            ShowTitle = popOutParams.ShowTitle;
            ShowInTaskBar = popOutParams.ShowInTaskBar;
            Count = popOutParams.Count;
            AdditionalSettings = popOutParams.AdditionalSettings;
            ZoomLevel = popOutParams.ZoomLevel;
        }

        internal void InvokeReset()
        {
            OnPopOutModeReset?.Invoke(this, Mode);
        }

        internal void ForceClose()
        {
            forceClose = true;
            var window = Window.GetWindow(this);
            window.Close();
        }
    }
}
