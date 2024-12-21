using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    /// <summary>
    /// Interaction logic for WorthMappingNotificationPart.xaml
    /// </summary>
    public partial class WorthMappingNotificationPart : NotificationDisplayPart
    {
        public WorthMappingNotificationPart(WorthMappingNotification mappingNotification)
        {
            InitializeComponent();
            Bind(mappingNotification);
        }

        private void NotificationDisplayPart_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is WorthMappingNotification notification)
            {
                notification.Options.NotificationClickAction.Invoke(notification);
                this.OnClose();
            }
        }
    }
}
