using LoadSaveSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ParserLibrary
{
    public class ExplorationTargets : INotifyPropertyChanged
    {
        #region Property Changed Logic
        // Declare the event
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private readonly string _previousSession = Path.Combine(Directory.GetCurrentDirectory(), "CSVSession.json");

        public event EventHandler<ExplorationTarget> OnCurrentTargetUpdated;
        public ObservableCollection<ExplorationTarget> Targets { get; private set; } = new();

        private ExplorationTarget _currentTarget;

        public ExplorationTarget CurrentTarget
        {
            get => _currentTarget;
            set
            {
                _currentTarget = value;
                OnPropertyChanged();
            }
        }

        private int _currentIndex;

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (Targets == null || Targets.Count < 1)
                {
                    _currentIndex = 0;
                    return;
                }

                _currentIndex = value;

                if (_currentIndex > Targets.Count - 1)
                {
                    _currentIndex = Targets.Count - 1;
                }
                else if (_currentIndex < 0)
                {
                    _currentIndex = 0;
                }

                CurrentTarget = Targets[_currentIndex];

                OnPropertyChanged("PreviousSystems");
                OnPropertyChanged("NextSystems");
                OnPropertyChanged("RemainingCount");

                OnCurrentTargetUpdated?.Invoke(this, CurrentTarget);
            }
        }

        public bool PreviousSystems
        {
            get
            {
                return Targets != null && Targets.Count >= 1 && _currentIndex > 0;
            }
        }

        public bool NextSystems
        {
            get
            {
                return Targets != null && Targets.Count >= 1 && _currentIndex < Targets.Count - 1;
            }
        }

        public int RemainingCount
        {
            get
            {
                return Targets == null || Targets.Count < 1 ? 0 : Targets.Count - _currentIndex;
            }
        }

        public bool LoadPreviousSession()
        {
            if (!File.Exists(_previousSession))
            {
                return false;
            }

            ExplorationSave prevSession = LoadSave.LoadJson<ExplorationSave>(_previousSession);

            if (prevSession == null || prevSession.Systems == null || prevSession.Systems.Count <= 0)
            {
                return false;
            }

            ListToObservableCollection(Targets,prevSession.Systems);
            CurrentIndex = prevSession.CurrentIndex;
            return true;
        }

        public void ImportCsv(string fileName)
        {
            ListToObservableCollection(Targets,CsvParser.ParseCsv(fileName));

            if (Targets == null && Targets.Count <= 0)
            {
                return;
            }

            CurrentIndex = 0;

            SaveState();
        }

        private static void ListToObservableCollection(ObservableCollection<ExplorationTarget> oc, List<ExplorationTarget> list)
        {
            oc.Clear();

            foreach (ExplorationTarget item in list)
            {
                oc.Add(item);
            }
        }

        public void SaveState()
        {
            ExplorationSave save = new(this);

            _ = LoadSave.SaveJson(save, _previousSession);
        }
    }
}