using System.Windows;

namespace ODExplorer.Utils.Helpers
{
    public static class XamlStringFormatHelper
    {
        #region Value

        public static DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            "Value", typeof(object), typeof(XamlStringFormatHelper), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RefreshFormattedValue(obj);
        }

        public static object GetValue(DependencyObject obj)
        {
            return obj.GetValue(ValueProperty);
        }

        public static void SetValue(DependencyObject obj, object newValue)
        {
            obj.SetValue(ValueProperty, newValue);
        }

        #endregion

        #region Format

        public static DependencyProperty FormatProperty = DependencyProperty.RegisterAttached(
            "Format", typeof(string), typeof(XamlStringFormatHelper), new PropertyMetadata(null, OnFormatChanged));

        private static void OnFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            RefreshFormattedValue(obj);
        }

        public static string GetFormat(DependencyObject obj)
        {
            return (string)obj.GetValue(FormatProperty);
        }

        public static void SetFormat(DependencyObject obj, string newFormat)
        {
            obj.SetValue(FormatProperty, newFormat);
        }

        #endregion

        #region FormattedValue

        public static DependencyProperty FormattedValueProperty = DependencyProperty.RegisterAttached(
            "FormattedValue", typeof(string), typeof(XamlStringFormatHelper), new PropertyMetadata(null));

        public static string GetFormattedValue(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedValueProperty);
        }

        public static void SetFormattedValue(DependencyObject obj, string newFormattedValue)
        {
            obj.SetValue(FormattedValueProperty, newFormattedValue);
        }

        #endregion

        private static void RefreshFormattedValue(DependencyObject obj)
        {
            var value = GetValue(obj);
            var format = GetFormat(obj);

            if (format != null)
            {
                if (!format.StartsWith("{0:"))
                {
                    format = string.Format("{{0:{0}}}", format);
                }

                SetFormattedValue(obj, string.Format(format, value));
            }
            else
            {
                SetFormattedValue(obj, value == null ? string.Empty : value.ToString());
            }
        }
    }
}
