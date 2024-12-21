using ODUtils.Dialogs.ViewModels;
using ODUtils.Journal;
using System.IO;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class JournalCommaderViewModel(JournalCommander cmdr) : OdViewModelBase
    {
        private readonly JournalCommander cmdr = cmdr;
        public string Name => cmdr.Name;
        public int Id => cmdr.Id;

        private string journalPath = cmdr.JournalPath ?? string.Empty;
        public string JournalPath
        {
            get => journalPath;
            set
            {
                journalPath = value;
                OnPropertyChanged(nameof(JournalPath));
            }
        }

        private string lasfile = Path.GetFileName(cmdr.LastFile ?? string.Empty);
        public string LastFile
        {
            get => lasfile;
            set
            {
                lasfile = value;
                OnPropertyChanged(nameof(LastFile));
            }
        }

        private bool isHidden = cmdr.IsHidden;
        public bool IsHidden
        {
            get => isHidden;
            set
            {
                isHidden = value;
                OnPropertyChanged(nameof(IsHidden));
            }
        }
    }
}
