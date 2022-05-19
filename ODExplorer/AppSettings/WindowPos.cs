using ODExplorer.Utils;
using System.Windows;

namespace ODExplorer.AppSettings
{
    public class WindowPos : PropertyChangeNotify
    {
        private double top;
        private double left;
        private double height = 450;
        private double width = 1440;
        private WindowState state = WindowState.Normal;

        public double Top { get => top; set { top = value; OnPropertyChanged(); } }
        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Height { get => height; set { height = value; OnPropertyChanged(); } }
        public double Width { get => width; set { width = value; OnPropertyChanged(); } }
        public WindowState State { get => state; set { state = value; OnPropertyChanged(); } }
    }
}