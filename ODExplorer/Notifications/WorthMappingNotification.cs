using ODExplorer.Extensions;
using ODExplorer.Models;
using ODUtils.Extensions;
using ODUtils.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    public class WorthMappingNotification : NotificationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private WorthMappingNotificationPart? _displayPart;

        public override NotificationDisplayPart? DisplayPart => _displayPart ??= new WorthMappingNotificationPart(this);

        public new MessageOptions Options { get; }
        public WorthMappingNotification(SystemBody body, NotificationSettings settings, MessageOptions options, string message = "") : base(message, options)
        {
            Options = options;
            Body = body;

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

        private SystemBody? body;
        public SystemBody? Body
        {
            get => body;
            set
            {
                body = value;
                OnPropertyChanged(nameof(Body));
            }
        }
        public string BodyName => body?.BodyName ?? "An error has occured";
        public string PlanetClass => body?.PlanetClass.GetEnumDescription() ?? "Please check the Error.txt file";
        public string MappValue => $"{body?.MappedValue ?? 0:N0} cr";
        public string Distance => $"{body?.DistanceFromArrivalLs ?? 0:N0} ls";
        public string? Discovered => body?.WasDiscovered.ToYesNo();
        public string? Mapped => body?.WasMapped.ToYesNo();
        public double? HeaderFontSize => Options.FontSize * 1.5;
        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public string BorderStyle { get; }
    }
}
