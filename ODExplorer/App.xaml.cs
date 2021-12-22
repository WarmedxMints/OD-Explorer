using ODExplorer.Themes;
using System.Windows;

namespace ODExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void ChangeSkin()
        {
            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {
                if (dict is ThemeManager skinDict)
                {
                    skinDict.UpdateSource();
                }
            }
        }

        public ResourceDictionary GetCurrentTheme()
        {
            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {
                if (dict is ThemeManager skinDict)
                {
                    return skinDict;
                }
            }
            return null;
        }
    }
}
