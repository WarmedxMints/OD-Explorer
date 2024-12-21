using ODExplorer.Models;
using ODExplorer.Stores;
using ODUtils.Models;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    public class ExoBioNotification : NotificationBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ExoBioNotification(OrganicScanItem item, string header, NotificationSettings settings, MessageOptions options, string message = "") : base(message, options)
        {
            this.item = item;
            this.header = header;
            this.settings = settings;
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

        private ExoBioNotificationPart? _displayPart;
        private readonly OrganicScanItem item;
        private readonly string header;
        private readonly NotificationSettings settings;

        public override ExoBioNotificationPart? DisplayPart => _displayPart ??= new(this);

        public new MessageOptions Options { get; }
        public string Header => string.IsNullOrEmpty(header) ? ItemName : $"{header}\n{ItemName}";
        public string ItemName
        {
            get
            {
                if (item.Info is null)
                    return string.Empty;

                var confirmed = item.Variants.FirstOrDefault(x => x.Confirmed);

                if (confirmed == null)
                {
                    return $"{item.SpeciesLocalised}";
                }
                return confirmed.EnglishName;
            }
        }
        public string EstimatedValue => $"Estimated Value : {item.TotalValue:N0} cr";
        public string ScanStage => item.ScanStage switch
        {
            EliteJournalReader.OrganicScanStage.Codex => "Codex Scan",
            EliteJournalReader.OrganicScanStage.Log => "Logged",
            EliteJournalReader.OrganicScanStage.Sample => "Sampled",
            EliteJournalReader.OrganicScanStage.Analyse => "Analysed",
            _ => string.Empty
        };
        public string ColonyRange => SettingsStore.Instance?.SystemGridSetting.DistanceUnit switch
        {
            Distance.Miles => item.Info is null ? string.Empty : $"Colony Range : {item.Info.ColonyRange * 3.280839895:N0} ft",
            _ => item.Info is null ? string.Empty : $"Colony Range : {item.Info.ColonyRange:N0} m",
        };
        public double? HeaderFontSize => Options.FontSize * 1.3;

        public Thickness TextMargin => Options.FontSize is null ? new(0, 0, 0, 2) : new(0, 0, 0, (double)Options.FontSize / 7);
        public Thickness BorderThickness { get; }
        public string BorderStyle { get; }
    }
}
