namespace ODExplorer.Extensions
{
    public static class BoolExtentions
    {
        public static string ToYesNo(this bool v)
        {
            return v ? "Yes" : "No";
        }
    }
}
