using ODExplorer.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    public class TestNotification : NotificationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TestNotification(string message, MessageOptions options, NotificationSettings settings) : base(message, options)
        {
            Options = options;

            var thinBorder = 2;
            var thickBorder = 6;

            switch (settings.Size)
            {
                case NotificationSize.Small:
                    thinBorder = 1;
                    thickBorder = 3;
                    break;
                case NotificationSize.Medium:
                    thinBorder = 2;
                    thickBorder = 6;
                    break;
                case NotificationSize.Large:
                    thinBorder = 3;
                    thickBorder = 9;
                    break;
            }

            switch (settings.DisplayRegion)
            {
                case ToastNotifications.Position.Corner.TopLeft:
                case ToastNotifications.Position.Corner.BottomLeft:
                    BorderThickness = new(thickBorder, thinBorder, thinBorder, thinBorder);
                    BorderStyle = "NotificationBorderStyleLeft";
                    break;
                default:
                    BorderThickness = new(thinBorder, thinBorder, thickBorder, thinBorder);
                    BorderStyle = "NotificationBorderStyle";
                    break;
            }
        }
        private TestNotificationPart? _displayPart;
        public override NotificationDisplayPart DisplayPart => _displayPart ??= new TestNotificationPart(this);
        public new MessageOptions Options { get; }
        public double? HeaderFontSize => Options.FontSize * 1.5;
        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public string BorderStyle { get; }

        public string ImageSource { get; } = "/Resources/Astronaut.png";
    }
}
