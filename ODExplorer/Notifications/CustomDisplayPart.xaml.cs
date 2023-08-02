using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    /// <summary>
    /// Interaction logic for CustomDisplayPart.xaml
    /// </summary>
    public partial class CustomDisplayPart : NotificationDisplayPart
    {
        public CustomDisplayPart(CustomNotification customNotification)
        {
            InitializeComponent();
            Bind(customNotification);
        }

        private void NotificationDisplayPart_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Notification.Close();
        }
    }
}
