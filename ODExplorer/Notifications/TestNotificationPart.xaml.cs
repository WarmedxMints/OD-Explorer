using ToastNotifications.Core;

namespace ODExplorer.Notifications
{
    /// <summary>
    /// Interaction logic for TestNotificationPart.xaml
    /// </summary>
    public partial class TestNotificationPart : NotificationDisplayPart
    {
        public TestNotificationPart(TestNotification testNotification)
        {
            InitializeComponent();
            Bind(testNotification);
        }

        private void NotificationDisplayPart_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is TestNotification testNotification)
            {
                testNotification.Options.NotificationClickAction.Invoke(testNotification);
                this.OnClose();
            }
        }
    }
}
