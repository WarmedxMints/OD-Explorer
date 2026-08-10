using ODExplorer.Models;
using ODExplorer.Stores;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Spansh;
using System.Collections.Generic;
using System.Linq;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class ExplorationTargetViewModel : OdViewModelBase
    {
        public ExplorationTargetViewModel(ExplorationTarget target, CsvType csvType, SpanshRouteProgressStore progressStore)
        {
            Target = target;
            this.progressStore = progressStore;
            BodiesInfo = target.BodiesInfo?.Select(x => new BodiesInfoViewModel(x, target.SystemName, csvType, progressStore)).ToList() ?? [];

            RefreshProgress();
        }

        private readonly SpanshRouteProgressStore progressStore;

        public ExplorationTarget Target { get; }

        public string SystemName => Target.SystemName;
        public string? Property1 => Target.Property1;
        public string? Property2 => Target.Property2;
        public string? Property3 => Target.Property3;
        public string? Property4 => Target.Property4;
        public List<BodiesInfoViewModel> BodiesInfo { get; private set; }

        private SpanshProgressState progress;
        public SpanshProgressState Progress
        {
            get => progress;
            private set
            {
                progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        /// <summary>
        /// Refreshes every row in this system and then rolls those rows up into the system's own state.
        /// </summary>
        internal void RefreshProgress()
        {
            foreach (var body in BodiesInfo)
            {
                body.RefreshProgress();
            }

            Progress = progressStore.RollUpSystemState(BodiesInfo.Select(x => x.Progress));
        }
    }
}
