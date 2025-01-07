using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    /// <summary>
    /// Interaction logic for SpanshNotificationPart.xaml
    /// </summary>
    public partial class SpanshNotificationPart : NotificationDisplayPart
    {
        public SpanshNotificationPart(SpanshNotification bioNotification)
        {
            InitializeComponent();
            Bind(bioNotification);
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
