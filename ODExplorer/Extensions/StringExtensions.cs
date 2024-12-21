using System.Globalization;

namespace ODExplorer.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string text)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }
    }
}
