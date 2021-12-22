using System.Windows;
using System.Windows.Input;
using ODExplorer.Themes;

namespace ODExplorer.AppSettings
{
    /// <summary>
    /// Interaction logic for ThemeEditor.xaml
    /// </summary>
    public partial class ThemeEditor : Window
    {
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
        }

        public ResourceDictionary CurrentDictionary { get; set; }

        public ThemeEditor()
        {
            CurrentDictionary = new();

            ResourceDictionary current = (Application.Current as App).GetCurrentTheme();

            foreach (var key in current.Keys)
            {
                CurrentDictionary.Add(key, current[key]);
            }

            InitializeComponent();
        }

        private void Save_Exit_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.SaveCustomTheme(CurrentDictionary);
            DialogResult = true;
        }
    }
}
