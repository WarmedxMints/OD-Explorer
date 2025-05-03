using ODExplorer.Models;
using ODUtils.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    public class NotableBodyNotification : NotificationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NotableBodyNotification(BodyNotification notification, SystemBody body, string header, string message, NotificationSettings settings, MessageOptions options) : base(message, options)
        {
            Options = options;
            Notification = notification;
            Header = header;
            BodyName = body.BodyName;

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

            ImageSource = notification switch
            {
                BodyNotification.WideRings or BodyNotification.LandableWithRings => "/Resources/NotificationIcons/saturn.png",
                BodyNotification.NestedMoon => "/Resources/NotificationIcons/nestedmoon.png",
                BodyNotification.ShepherdMoon => "/Resources/NotificationIcons/shepherdmoon.png",
                BodyNotification.BioSignals => "/Resources/exobtn.png",
                BodyNotification.GeoSignals => "/Resources/NotificationIcons/volcano.png",
                _ => "/Resources/NotificationIcons/planet.png",
            };
        }

        private NotableBodyNotificationPart? _displayPart;
        public override NotableBodyNotificationPart? DisplayPart => _displayPart ??= new(this);

        public new MessageOptions Options { get; }      
       
        public double? HeaderFontSize => Options.FontSize * 1.3;

        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public string BorderStyle { get; }
        public BodyNotification Notification { get; }
        public string Header { get; }
        public string BodyName { get; }
        public string ImageSource { get; }
    }
}
