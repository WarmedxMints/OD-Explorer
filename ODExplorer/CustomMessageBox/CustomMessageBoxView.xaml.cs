using ODExplorer.Utils;
using System.Media;
using System.Windows;
using System.Windows.Input;

namespace ODExplorer.CustomMessageBox
{
    /// <summary>
    /// Interaction logic for CustomMessageBoxView.xaml
    /// </summary>
    public partial class CustomMessageBoxView : Window
    {
        internal string WindowTitle
        {
            get => Title;
            set => Title = value;
        }
        internal string Message
        {
            get => MessageText.Text;
            set => MessageText.Text = value;
        }

        internal string YesButtonText
        {
            get => Label_Yes.Content.ToString();
            set => Label_Yes.Content = value.TryAddKeyboardAccellerator();
        }

        internal string NoButtonText
        {
            get => Label_No.Content.ToString();
            set => Label_No.Content = value.TryAddKeyboardAccellerator();
        }

        public MessageBoxResult Result { get; set; }

        public CustomMessageBoxView(string message, string yesButtonText, string noButtonText, MessageBoxButton messageBoxButton, string title = "CONFIRMATION")
        {
            InitializeComponent();

            Title = title;
            Message = message;
            YesButtonText = yesButtonText;
            NoButtonText = noButtonText;
            DisplayButtons(messageBoxButton);

            SystemSounds.Beep.Play();
        }

        private void DisplayButtons(MessageBoxButton messageBoxButton)
        {
            switch (messageBoxButton)
            {
                case MessageBoxButton.OKCancel:
                    // Hide all but OK, Cancel
                    Button_OK.Visibility = Visibility.Visible;
                    _ = Button_Cancel.Focus();
                    Button_Cancel.Visibility = Visibility.Visible;

                    Button_Yes.Visibility = Visibility.Collapsed;
                    Button_No.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    // Hide all but Yes, No
                    Button_Yes.Visibility = Visibility.Visible;
                    _ = Button_No.Focus();
                    Button_No.Visibility = Visibility.Visible;

                    Button_OK.Visibility = Visibility.Collapsed;
                    Button_Cancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    // Hide only OK
                    Button_Yes.Visibility = Visibility.Visible;
                    _ = Button_Cancel.Focus();
                    Button_No.Visibility = Visibility.Visible;
                    Button_Cancel.Visibility = Visibility.Visible;

                    Button_OK.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OK:
                default:
                    // Hide all but OK
                    Button_OK.Visibility = Visibility.Visible;
                    _ = Button_OK.Focus();

                    Button_Yes.Visibility = Visibility.Collapsed;
                    Button_No.Visibility = Visibility.Collapsed;
                    Button_Cancel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            SystemCommands.CloseWindow(this);
        }
    }
}