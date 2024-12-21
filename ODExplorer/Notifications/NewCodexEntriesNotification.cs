using ODExplorer.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    public class NewCodexEntriesNotification : NotificationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public NewCodexEntriesNotification(NotificationSettings settings,
                                           MessageOptions options,
                                           string bodyName,
                                           Dictionary<string, bool> newEntries,
                                           string message,
                                           string? currentSystemRegion) : base(message, options)
        {
            this.settings = settings;
            Options = options;
            BodyName = bodyName;
            Region = $"In {currentSystemRegion}";
            NewEntries = newEntries.Select(x => new CodexNotificationItem(x.Key, x.Value)).ToList(); ;
            var thinBorder = 2;
            var thickBorder = 6;

            switch (this.settings.Size)
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

        private NewCodexEntriesNotificationPart? _displayPart;
        private readonly NotificationSettings settings;
        public new MessageOptions Options { get; }
        public string BodyName { get; }
        public string? Region { get; }
        public List<CodexNotificationItem> NewEntries { get; }

        public override NewCodexEntriesNotificationPart? DisplayPart => _displayPart ??= new(this);
        public Thickness BorderThickness { get; }
        public string BorderStyle { get; }
        public double? HeaderFontSize => Options.FontSize * 1.3;
        public Thickness TextMargin => Options.FontSize is null ? new(0, 2, 0, 2) : new(0, 3, 0, (double)Options.FontSize / 7);

    }
}
