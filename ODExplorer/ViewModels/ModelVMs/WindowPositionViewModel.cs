using ODUtils.Dialogs.ViewModels;
using System.Runtime.Serialization;
using System.Windows;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class WindowPositionViewModel : OdViewModelBase
    {
        private double top;
        private double left;
        private double height;
        private double width;
        private WindowState state = WindowState.Normal;

        public double Top { get => top; set { top = value; OnPropertyChanged(); } }
        public double Left { get => left; set { left = value; OnPropertyChanged(); } }
        public double Height { get => height; set { height = value; OnPropertyChanged(); } }
        public double Width { get => width; set { width = value; OnPropertyChanged(); } }
        public WindowState State { get => state; set { state = value; OnPropertyChanged(); } }
        [IgnoreDataMember]
        public bool IsZero => Top == 0 && Left == 0 && Height == 0 && Width == 0;

        public WindowPositionViewModel Clone()
        {
            return new()
            {
                Top = Top,
                Left = Left,
                Height = Height,
                Width = Width,
                State = State
            };
        }
    }
}
