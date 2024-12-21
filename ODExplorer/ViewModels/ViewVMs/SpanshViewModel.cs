using ODExplorer.Extensions;
using ODExplorer.Models;
using ODExplorer.Stores;
using ODExplorer.ViewModels.ModelVMs;
using ODUtils.Commands;
using ODUtils.Dialogs.ViewModels;
using ODUtils.Spansh;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ODExplorer.ViewModels.ViewVMs
{


    public sealed class SpanshViewModel : OdViewModelBase
    {
        private readonly SpanshCsvStore csvStore;
        private readonly SettingsStore settingsStore;
        private readonly NotificationStore notificationStore;
        private ExplorationTargetViewModel? currentTarget;
        private ExplorationTargetViewModel? nextTarget;

        public SpanshViewModel(SpanshCsvStore csvStore, SettingsStore settingsStore, NotificationStore notificationStore)
        {
            this.csvStore = csvStore;
            this.settingsStore = settingsStore;
            this.notificationStore = notificationStore;
            this.csvStore.OnCurrentTargetChanged += OnCurrentTargetChanged;
            this.csvStore.OnCurrentContainerChanged += CsvStore_OnCurrentContainerChanged;

            var currentContainer = this.csvStore.GetCurrentContainer(this.settingsStore.SpanshCSVSettings[this.settingsStore.SelectedCommanderID]);
            CsvStore_OnCurrentContainerChanged(null, currentContainer);

            csvStore.OnCarrierTimerRunning += CsvStore_OnCarrierTimerRunning;
            csvStore.OnCarrierTimeTick += CsvStore_OnCarrierTimeTick;

            PreviousTargetCommand = new RelayCommand(SelectPreviousTarget, (_) => CurrentIndex > 0);
            NextTargetCommand = new RelayCommand(SelectNextTarget, (_) => NextTarget != null);
            SetCurrentCSVType = new RelayCommand<CsvType>(SetCSVType, (csvType) => CurrentType != csvType);
            CopyToClipboard = new RelayCommand<ExplorationTargetViewModel>(CopySystemNameToClipboard, (targetVM) => targetVM != null);
            StartStopFleetCarrierTimer = new RelayCommand<bool>(OnStartStopFCTimer, (v) => v != CarrierTimerRunning);
        }

        private void CsvStore_OnCarrierTimeTick(object? sender, string e)
        {
            Application.Current.Dispatcher.Invoke(() => CarrierTimerString = e);

        }

        private void CsvStore_OnCarrierTimerRunning(object? sender, bool e)
        {
            Application.Current.Dispatcher.Invoke(() => OnPropertyChanged(nameof(CarrierTimerRunning)));
        }

        private void SetCSVType(CsvType type)
        {
            CurrentType = type;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public ObservableCollection<ExplorationTargetViewModel> Targets { get; private set; } = [];
        public ExplorationTargetViewModel? CurrentTarget { get => currentTarget; private set => currentTarget = value; }
        public ExplorationTargetViewModel? NextTarget { get => nextTarget; private set => nextTarget = value; }
        public int CurrentIndex => csvStore.CurrentIndex;
        public CsvType CurrentType
        {
            get => settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID];
            set
            {
                settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID] = value;
                CsvStore_OnCurrentContainerChanged(null, csvStore.GetCurrentContainer(value));
                OnPropertyChanged(nameof(CurrentType));
            }
        }
        public int RemainingCount => Targets.Count > 0 ? Targets.Count - CurrentIndex - 1 : 0;
        public bool CarrierTimerRunning => csvStore.CarrierTimerRunning;
        private string carrierTimerString = "00:00";
        public string CarrierTimerString
        {
            get => carrierTimerString;
            set
            {
                carrierTimerString = value;
                OnPropertyChanged(nameof(CarrierTimerString));
            }
        }

        public bool AutoCopyNextSystem
        {
            get => settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard;
            set => settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard = value;
        }
        public bool AutoStartFcTimer
        {
            get => settingsStore.SpanshCSVSettings.AutoStartFleetCarrierTimer;
            set => settingsStore.SpanshCSVSettings.AutoStartFleetCarrierTimer = value;
        }

        public bool FcTimerNotification
        {
            get => settingsStore.SpanshCSVSettings.NotifyOnFcTimerCompletion;
            set => settingsStore.SpanshCSVSettings.NotifyOnFcTimerCompletion = value;
        }

        public string? CustomAudioFile
        {
            get
            {
                if (string.IsNullOrEmpty(settingsStore.SpanshCSVSettings.CustomTimerSound))
                    return string.Empty;
                return Path.GetFileName(settingsStore.SpanshCSVSettings.CustomTimerSound);
            }
        }

        public void SetCustomFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            settingsStore.SpanshCSVSettings.CustomTimerSound = filename;
            OnPropertyChanged(nameof(CustomAudioFile));
        }

        public ICommand PreviousTargetCommand { get; }
        public ICommand NextTargetCommand { get; }
        public ICommand SetCurrentCSVType { get; }
        public ICommand CopyToClipboard { get; }
        public ICommand StartStopFleetCarrierTimer { get; }

        public event EventHandler<SpanshCsvErrorEventArgs>? OnErrorProcessingCSV;

        private void OnCurrentTargetChanged(object? sender, ExplorationTarget? e)
        {
            if (CurrentIndex < 0)
            {
                currentTarget = null;
                nextTarget = null;
                OnPropertyChanged(nameof(CurrentTarget));
                OnPropertyChanged(nameof(NextTarget));
                OnPropertyChanged(nameof(CurrentIndex));
                OnPropertyChanged(nameof(RemainingCount));
                return;
            }
            currentTarget = CurrentIndex < Targets.Count - 1 ? Targets[CurrentIndex]
                : Targets.FirstOrDefault(x => x.SystemName == csvStore.CurrentTarget?.SystemName);

            nextTarget = Targets.FirstOrDefault(x => x.SystemName == csvStore.NextTarget?.SystemName);
            OnPropertyChanged(nameof(CurrentTarget));
            OnPropertyChanged(nameof(NextTarget));
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(RemainingCount));
        }

        private void CsvStore_OnCurrentContainerChanged(object? sender, SpanshCsvContainer? e)
        {
            CurrentTarget = null;
            NextTarget = null;
            Targets.Clear();

            if (e is null)
            {
                return;
            }

            Targets.AddRangeToCollection(csvStore.CurrentContainer?.Targets.Select(x => new ExplorationTargetViewModel(x)) ?? []);
            OnPropertyChanged(nameof(Targets));
            OnCurrentTargetChanged(null, csvStore.CurrentTarget);
        }

        private void SelectNextTarget(object? obj)
        {
            csvStore.CurrentIndex++;
        }

        private void SelectPreviousTarget(object? obj)
        {
            csvStore.CurrentIndex--;
        }

        private void CopySystemNameToClipboard(ExplorationTargetViewModel model)
        {
            if (model == null)
            {
                return;
            }

            notificationStore.CopyToClipBoard(model.SystemName);
        }

        public void ParseCSV(string fileName)
        {
            var csv = csvStore.ParseCSV(fileName);

            if (csv)
            {
                OnPropertyChanged(nameof(CurrentType));
                return;
            }

            OnErrorProcessingCSV?.Invoke(this, new(fileName, SpanshCSVError.Parse));
        }

        public void ForceParseCSV(string fileName, CsvType csvType)
        {
            var csv = csvStore.ForceParseCSV(fileName, csvType);

            if (csv)
            {
                OnPropertyChanged(nameof(CurrentType));
                return;
            }

            OnErrorProcessingCSV?.Invoke(this, new(fileName, SpanshCSVError.ForcePass));
        }

        private void OnStartStopFCTimer(bool obj)
        {
            if (obj)
            {
                csvStore.StartFleetCarrierTimer();
                return;
            }

            csvStore.StopFleetCarrierTimer();
        }
    }
}
