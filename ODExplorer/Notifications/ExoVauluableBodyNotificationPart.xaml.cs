using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    /// <summary>
    /// Interaction logic for ExoValuableBodyNotificationPart.xaml
    /// </summary>
    public partial class ExoValuableBodyNotificationPart : NotificationDisplayPart
    {
        public ExoValuableBodyNotificationPart(ExoValuableBodyNotification exoValuableBodyNotification)
        {
            InitializeComponent();
            Bind(exoValuableBodyNotification);

        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnClose();
        }

        private void NotificationDisplayPart_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ExoBioNotification notification)
            {
                notification.Options.NotificationClickAction.Invoke(notification);
                this.OnClose();
            }
        }
    }
}

