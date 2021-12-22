using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using ODExplorer.AppSettings;
using ODExplorer.Utils;

namespace ODExplorer.Themes
{
    public class ThemeManager : ResourceDictionary
    {
        private static readonly string saveFile = Path.Combine(Directory.GetCurrentDirectory(), "CustomTheme.json");

        private static ResourceDictionary customTheme = new() { Source = new Uri(EnumDescriptionConverter.GetEnumDescription(Theme.Custom), UriKind.Absolute) };

        private Uri _defaultSource;
        public Uri DefaultSource
        {
            get => _defaultSource;
            set
            {
                _defaultSource = value;
                LoadCustomThemeFromSave();
                UpdateSource();
            }
        }

        public void UpdateSource()
        {
            if (Settings.CurrentTheme == Theme.Custom)
            {
                Source = new Uri(EnumDescriptionConverter.GetEnumDescription(Theme.Custom), UriKind.Absolute);

                foreach (object key in customTheme.Keys)
                {
                    this[key] = customTheme[key];
                }
                return;
            }

            Uri val = new(EnumDescriptionConverter.GetEnumDescription(Settings.CurrentTheme), UriKind.Absolute);

            if (val != null && CheckCurrentSource(val))
            {
                Source = val;
            }
        }

        public static void LoadCustomThemeFromSave()
        {
            if (!File.Exists(saveFile))
            {
                return;
            }

            ResourceDictionary save = LoadSaveSystem.LoadSave.LoadJson<ResourceDictionary>(saveFile);

            foreach (object key in save.Keys)
            {
                customTheme[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(save[key].ToString()));
            }
        }
        public static void SaveCustomTheme(ResourceDictionary source)
        {
            foreach (object key in source.Keys)
            {
                customTheme[key] = source[key]; ;
            }

            _ = LoadSaveSystem.LoadSave.SaveJson(customTheme, saveFile);
        }

        public static ResourceDictionary GetDictionary(Theme theme)
        {
            return new ResourceDictionary() { Source = new Uri(EnumDescriptionConverter.GetEnumDescription(theme), UriKind.Absolute) };
        }

        private bool CheckCurrentSource(Uri val)
        {
            return Source is null || Source.AbsoluteUri != val.AbsoluteUri;
        }
    }
}
