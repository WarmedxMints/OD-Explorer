using ToastNotifications.Core;
using ToastNotifications;
using System.Windows;

namespace ODExplorer.Notifications
{
    public static class CustomMessageExtensions
    {
        public static void ShowCustomMessage(this Notifier notifier, string header, string message, MessageOptions options)
        {
            notifier.Notify(() => new CustomNotification(header, message, options));
        }

        public static void ShowCustomMessageOnMainThread(this Notifier notifier, string header, string message, MessageOptions options)
        {
            Application.Current.Dispatcher.Invoke(()=> notifier.Notify(() => new CustomNotification(header, message, options)));
        }
    }
}
