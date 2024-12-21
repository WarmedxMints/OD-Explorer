namespace ODExplorer.Notifications
{
    public class CodexNotificationItem(string name, bool newSpecies)
    {
        public string Name { get; } = name;
        public bool NewSpecies { get; } = newSpecies;
    }
}
