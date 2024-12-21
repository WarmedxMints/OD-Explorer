using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    /// <summary>
    /// Interaction logic for EDSMValuableBodiesNotificationPart.xaml
    /// </summary>
    public partial class EDSMValuableBodiesNotificationPart : NotificationDisplayPart
    {
        public EDSMValuableBodiesNotificationPart(EDSMValuableBodiesNotification notification)
        {
            InitializeComponent();
            Bind(notification);
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
