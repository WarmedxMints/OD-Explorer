using ODExplorer.Models;
using ODExplorer.Stores;
using ODUtils.Commands;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Models;
using System.Windows.Input;
using ToastNotifications.Position;

namespace ODExplorer.ViewModels.ViewVMs
{
    public sealed class DisplaySettingsViewModel : OdViewModelBase
    {
        public DisplaySettingsViewModel(SettingsStore settingsStore, NotificationStore notificationStore)
        {
            this.settingsStore = settingsStore;
            this.notificationStore = notificationStore;
            ClonedSettings = settingsStore.NotificationSettings.Clone();

            SetSmallNotifications = new RelayCommand(SetNotificationSmall, (_) => NotificationSize != NotificationSize.Small);
            SetMediumNotifications = new RelayCommand(SetNotificationMedium, (_) => NotificationSize != NotificationSize.Medium);
            SetLargeNotifications = new RelayCommand(SetNotificationLarge, (_) => NotificationSize != NotificationSize.Large);

            SetNotifyPositionBottomRight = new RelayCommand(SetNotificationPosBottomRight, (_) => NotificationPosition != Corner.BottomRight);
            SetNotifyPositionBottomLeft = new RelayCommand(SetNotificationPosBottomLeft, (_) => NotificationPosition != Corner.BottomLeft);
            SetNotifyPositionTopRight = new RelayCommand(SetNotificationPosTopRight, (_) => NotificationPosition != Corner.TopRight);
            SetNotifyPositionTopLeft = new RelayCommand(SetNotificationPosTopLeft, (_) => NotificationPosition != Corner.TopLeft);

            SetNotificationDefaults = new RelayCommand(SetDefaultNotificationValues, (_) => !ClonedSettings.Equals(defaultSettings));
            SaveNotificationSettings = new RelayCommand(OnSaveNotificationSetting, (_) => ClonedSettings.Equals(settingsStore.NotificationSettings) == false);
            TestNotificationCommand = new RelayCommand(OnTestNotifications, (_) => ClonedSettings.Equals(settingsStore.NotificationSettings) && EnableNotifications);

            ResetBodyNotificationsSettingsCommand = new RelayCommand(OnResetBodySettings, (_) => WorthMappingDistance != 0 || WorthMappingValue != 500_000);
            ResetBioBodyValueCommand = new RelayCommand(OnResetBioBodyValue, (_) => ExoValuableBodyValue != 20_000_000);
        }

        private readonly SettingsStore settingsStore;
        private readonly NotificationStore notificationStore;
        private readonly NotificationSettings defaultSettings = NotificationSettings.GetDefault();

        #region Properties
        public double SmallRadius
        {
            get => NotableSettings.SmallRadius;
            set
            {
                NotableSettings.SmallRadius = value;
                OnPropertyChanged(nameof(SmallRadius));
                OnPropertyChanged(nameof(SmallRadiusStringConversion));
            }
        }
        public string SmallRadiusStringConversion
        {
            get
            {
                return settingsStore.SystemGridSetting.DistanceUnit switch
                {
                    Distance.Miles => $"{SmallRadius * 0.62137:N1} mi",
                    _ => $"{SmallRadius:N0} km"
                };
            }
        }

        public double LargeRadius
        {
            get => NotableSettings.LargeRadius;
            set
            {
                NotableSettings.LargeRadius = value;
                OnPropertyChanged(nameof(LargeRadius));
                OnPropertyChanged(nameof(LargeRadiusStringConversion));
            }
        }
        public string LargeRadiusStringConversion
        {
            get
            {
                return settingsStore.SystemGridSetting.DistanceUnit switch
                {
                    Distance.Miles => $"{LargeRadius * 0.62137:N1} mi",
                    _ => $"{LargeRadius:N0} km"
                };
            }
        }
        public CodexEntryHistory CodexEntryHistory
        {
            get => settingsStore.CodexEntryHistory;
            set => settingsStore.CodexEntryHistory = value;
        }
        public NotificationSettings ClonedSettings { get; set; }

        public NotificationOptions NotifyOptions
        {
            get => settingsStore.NotificationOptions;
            set
            {
                settingsStore.NotificationOptions = value;
                OnPropertyChanged(nameof(NotifyOptions));
            }
        }

        public NotableNotificationOptions NotableSettings => settingsStore.NotableSettings;

        public NotificationSize NotificationSize
        {
            get => ClonedSettings.Size;
            set
            {
                ClonedSettings.Size = value;
                OnPropertyChanged(nameof(NotificationSize));
                OnPropertyChanged(nameof(ClonedSettings));
            }
        }

        public Corner NotificationPosition
        {
            get => ClonedSettings.DisplayRegion;
            set
            {
                ClonedSettings.DisplayRegion = value;
                OnPropertyChanged(nameof(NotificationPosition));
                OnPropertyChanged(nameof(ClonedSettings));
            }
        }

        public int NotificationDuration
        {
            get => ClonedSettings.DisplayTime;
            set
            {
                ClonedSettings.DisplayTime = value;
                OnPropertyChanged(nameof(NotificationDuration));
                OnPropertyChanged(nameof(ClonedSettings));
            }
        }

        public int MaxNotificationCount
        {
            get => ClonedSettings.MaxNotificationCount;
            set
            {
                ClonedSettings.MaxNotificationCount = value;
                OnPropertyChanged(nameof(MaxNotificationCount));
                OnPropertyChanged(nameof(ClonedSettings));
            }
        }

        public int XOffset
        {
            get => ClonedSettings.XOffset;
            set
            {
                ClonedSettings.XOffset = value;
                OnPropertyChanged(nameof(XOffset));
                OnPropertyChanged(nameof(ClonedSettings));
            }
        }
        public int YOffset
        {
            get => ClonedSettings.YOffset;
            set
            {
                ClonedSettings.YOffset = value;
                OnPropertyChanged(nameof(YOffset));
                OnPropertyChanged(nameof(ClonedSettings));
            }
        }

        public int WorthMappingValue
        {
            get => settingsStore.SystemGridSetting.ValuableBodyValue;
            set
            {
                settingsStore.SystemGridSetting.ValuableBodyValue = value;
                OnPropertyChanged(nameof(WorthMappingValue));
            }
        }

        public int WorthMappingDistance
        {
            get => settingsStore.SystemGridSetting.ValuableBodyDistance;
            set
            {
                settingsStore.SystemGridSetting.ValuableBodyDistance = value;
                OnPropertyChanged(nameof(WorthMappingDistance));
            }
        }

        public int ExoValuableBodyValue
        {
            get => settingsStore.SystemGridSetting.ExoValuableBodyValue;
            set
            {
                settingsStore.SystemGridSetting.ExoValuableBodyValue = value;
                OnPropertyChanged(nameof(ExoValuableBodyValue));
            }
        }

        public bool EnableNotifications
        {
            get => ClonedSettings.NotificationsEnabled;
            set
            {
                ClonedSettings.NotificationsEnabled = value;
                OnPropertyChanged(nameof(EnableNotifications));
            }
        }
        #endregion

        #region Commands and Methods
        public ICommand SetSmallNotifications { get; }
        public ICommand SetMediumNotifications { get; }
        public ICommand SetLargeNotifications { get; }
        public ICommand SetNotifyPositionBottomRight { get; }
        public ICommand SetNotifyPositionBottomLeft { get; }
        public ICommand SetNotifyPositionTopRight { get; }
        public ICommand SetNotifyPositionTopLeft { get; }
        public ICommand SetNotificationDefaults { get; }
        public ICommand SaveNotificationSettings { get; }
        public ICommand TestNotificationCommand { get; }
        public ICommand ResetBodyNotificationsSettingsCommand { get; }
        public ICommand ResetBioBodyValueCommand { get; }

        private void SetNotificationSmall(object? obj)
        {
            NotificationSize = NotificationSize.Small;
        }

        private void SetNotificationMedium(object? obj)
        {
            NotificationSize = NotificationSize.Medium;
        }

        private void SetNotificationLarge(object? obj)
        {
            NotificationSize = NotificationSize.Large;
        }

        private void SetNotificationPosBottomRight(object? obj)
        {
            NotificationPosition = Corner.BottomRight;
        }

        private void SetNotificationPosBottomLeft(object? obj)
        {
            NotificationPosition = Corner.BottomLeft;
        }

        private void SetNotificationPosTopRight(object? obj)
        {
            NotificationPosition = Corner.TopRight;
        }

        private void SetNotificationPosTopLeft(object? obj)
        {
            NotificationPosition = Corner.TopLeft;
        }

        private void OnSaveNotificationSetting(object? obj)
        {
            settingsStore.NotificationSettings = ClonedSettings.Clone();
            notificationStore.ChangeNotifierSetting(settingsStore.NotificationSettings);
            OnPropertyChanged(nameof(ClonedSettings));
        }

        private void OnTestNotifications(object? obj)
        {
            notificationStore.ShowTestNotification();
        }

        private void OnResetBodySettings(object? obj)
        {
            WorthMappingDistance = 0;
            WorthMappingValue = 500_000;
        }

        private void OnResetBioBodyValue(object? obj)
        {
            ExoValuableBodyValue = 20_000_000;
        }

        private void SetDefaultNotificationValues(object? obj)
        {
            ClonedSettings = NotificationSettings.GetDefault();
            OnPropertyChanged(nameof(NotificationSize));
            OnPropertyChanged(nameof(NotifyOptions));
            OnPropertyChanged(nameof(NotificationPosition));
            OnPropertyChanged(nameof(NotificationDuration));
            OnPropertyChanged(nameof(MaxNotificationCount));
            OnPropertyChanged(nameof(XOffset));
            OnPropertyChanged(nameof(YOffset));
            OnPropertyChanged(nameof(ClonedSettings));
        }
        #endregion
    }
}
