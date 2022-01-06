namespace ODExplorer.AppSettings.NoteableBody
{
    public class NoteableMenuItemClickArgs
    {
        public NoteableMenuItem Preset { get; private set; }

        public NoteableMenuItemClickArgs(NoteableMenuItem preset)
        {
            Preset = preset;
        }
    }
}