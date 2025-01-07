using System;
using ToastNotifications.Position;

namespace ODExplorer.Models
{
    public enum NotificationSize
    {
        Small,
        Medium,
        Large,
    }

    [Flags]
    public enum NotificationOptions
    {
        None = 0,
        WorthMapping = 1 << 0,
        NewBioScanned = 1 << 1,
        DistanceFromBio = 1 << 2,
        ValuableBioPlanet = 1 << 3,
        NewBioCodexEntry = 1 << 4,
        CopyToClipboard = 1 << 5,
        EDSMValuableBody = 1 << 6,
        NewBioSpecies = 1 << 7,       
        SpanshCSV = 1 << 8,       
        All = ~(-1 << 9)
    }

    public sealed class NotificationSettings
    {
        public int DisplayTime { get; set; }
        public Corner DisplayRegion { get; set; }
        public NotificationSize Size { get; set; }
        public int MaxNotificationCount { get; set; }
        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public bool NotificationsEnabled { get; set; } = true;
        public static NotificationSettings GetDefault()
        {
            return new()
            {
                DisplayTime = 10,
                DisplayRegion = Corner.BottomRight,
                Size = NotificationSize.Medium,
                MaxNotificationCount = 8,
                XOffset = 40,
                YOffset = 20,
                NotificationsEnabled = true
            };
        }

        public NotificationSettings Clone()
        {
            return new()
            {
                DisplayTime = this.DisplayTime,
                DisplayRegion = this.DisplayRegion,
                Size = this.Size,
                MaxNotificationCount = this.MaxNotificationCount,
                XOffset = this.XOffset,
                YOffset = this.YOffset,
                NotificationsEnabled = this.NotificationsEnabled,
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj is not NotificationSettings setting)
                return false;

            return DisplayTime == setting.DisplayTime
                && DisplayRegion == setting.DisplayRegion
                && Size == setting.Size
                && MaxNotificationCount == setting.MaxNotificationCount
                && XOffset == setting.XOffset
                && YOffset == setting.YOffset
                && NotificationsEnabled == setting.NotificationsEnabled;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
