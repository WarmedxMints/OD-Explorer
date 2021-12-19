using System;
using System.Windows;
using ODExplorer.AppSettings;
using ODExplorer.Utils;

namespace ODExplorer.Themes
{
    public class ThemeManager : ResourceDictionary
    {
        private Uri _defaultSource;
        public Uri DefaultSource
        {
            get => _defaultSource;
            set
            {
                _defaultSource = value;
                UpdateSource();
            }
        }

        public void UpdateSource()
        {
            Uri val = new(EnumDescriptionConverter.GetEnumDescription(Settings.CurrentTheme), UriKind.Absolute);

            if (val != null && CheckCurrentSource(val))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Source = val;
                });
            }
        }

        private bool CheckCurrentSource(Uri val)
        {
            return Source is null || Source.AbsoluteUri != val.AbsoluteUri;
        }
    }
}
