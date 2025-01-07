using ODExplorer.Models;
using ODUtils.Spansh;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    public enum SpanshNotificationType
    {
        Refuel
    }

    public class SpanshNotification : NotificationBase, INotifyPropertyChanged
    {
        private SpanshNotificationPart? _displayPart;

        public override SpanshNotificationPart DisplayPart => _displayPart ??= new(this);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SpanshNotification(NotificationSettings settings, MessageOptions options, SpanshNotificationType csvType) : base(string.Empty, options)
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

            switch (csvType)
            {
                case SpanshNotificationType.Refuel:
                    ImageSource = "/Resources/NotificationIcons/refuel.png";
                    Message = "Refuel System";
                    break;
                default:
                    ImageSource = string.Empty;
                    Message = string.Empty;
                    break;
            }
        }

        public new string Message { get; }
        public new MessageOptions Options { get; }
        public double? HeaderFontSize => Options.FontSize * 1.2;
        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public string BorderStyle { get; }
        public string ImageSource { get; }
    }
}