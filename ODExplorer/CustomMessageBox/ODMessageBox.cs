using System.Windows;

namespace ODExplorer.CustomMessageBox
{
    public static class ODMessageBox
    {
        public static MessageBoxResult Show(Window owner, string message, string yesButtonText = "_YES", string noButtonText = "_NO", MessageBoxButton messageBoxButton = MessageBoxButton.OK)
        {
            CustomMessageBoxView msg = new(message, yesButtonText, noButtonText, messageBoxButton);
            msg.Owner = owner;
            _ = msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(Window owner, string message, MessageBoxButton messageBoxButton)
        {
            CustomMessageBoxView msg = new(message, "_YES", "_NO", messageBoxButton);
            msg.Owner = owner;
            _ = msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(Window owner, string message, MessageBoxButton messageBoxButton, string title)
        {
            CustomMessageBoxView msg = new(message, "_YES", "_NO", messageBoxButton, title);
            msg.Owner = owner;
            _ = msg.ShowDialog();

            return msg.Result;
        }

        public static MessageBoxResult Show(Window owner, string message, string title)
        {
            CustomMessageBoxView msg = new(message, "_YES", "_NO", MessageBoxButton.OK, title);
            msg.Owner = owner;
            _ = msg.ShowDialog();

            return msg.Result;
        }
    }
}
