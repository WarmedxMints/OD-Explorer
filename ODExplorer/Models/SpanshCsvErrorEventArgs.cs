namespace ODExplorer.Models
{
    public enum SpanshCSVError
    {
        Parse,
        ForcePass
    }

    public sealed class SpanshCsvErrorEventArgs(string filename, SpanshCSVError errorType)
    {
        public string Filename { get; } = filename;
        public SpanshCSVError ErrorType { get; } = errorType;
    }
}
