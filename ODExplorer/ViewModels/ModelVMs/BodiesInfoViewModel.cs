using ODExplorer.Models;
using ODExplorer.Stores;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Spansh;

namespace ODExplorer.ViewModels.ModelVMs
{
    public sealed class BodiesInfoViewModel(BodiesInfo info,
                                            string systemName,
                                            CsvType csvType,
                                            SpanshRouteProgressStore progressStore) : OdViewModelBase
    {
        public BodiesInfo Info { get; } = info;
        public string? Body => Info.Body;
        public string? Distance => Info.Distance;
        public string? Property1 => Info.Property1;

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
        /// Re-reads how much of this entry the commander has completed. For an exobiology route the species is
        /// carried in <see cref="Distance"/>, the other route types have no species and ignore it.
        /// </summary>
        internal void RefreshProgress()
        {
            Progress = progressStore.GetBodyState(csvType, systemName, Body, Distance);
        }
    }
}
