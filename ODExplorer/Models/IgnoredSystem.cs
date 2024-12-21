namespace ODExplorer.Models
{
    public sealed class IgnoredSystem(long address, string name, int cmdrId)
    {
        public long Address { get; } = address;
        public string Name { get; } = name;
        public int CmdrId { get; } = cmdrId;
    }
}
