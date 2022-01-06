using System.Windows;
using System.Windows.Input;

namespace ODExplorer.TextInputBox
{
    /// <summary>
    /// Interaction logic for TextInputMessageBox.xaml
    /// </summary>
    public partial class TextInputMessageBox : Window
    {
        public TextInputMessageBox()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ = InputTextBox.Focus();
            InputTextBox.SelectAll();
        }
        public string InputText => InputTextBox.Text;

        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            SystemCommands.CloseWindow(this);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
