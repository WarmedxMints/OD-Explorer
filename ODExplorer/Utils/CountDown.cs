using System;
using System.Threading;
using System.Threading.Tasks;
namespace ODExplorer.Utils
{
    public class Countdown : PropertyChangeNotify
    {
        private string _currentTimeString = "00:00";
        private TimeSpan _runTime;
        private bool _shouldStop;
        private DateTime _timeToStop;
        private TimeSpan _updateInterval;

        public Countdown(TimeSpan runTime)
        {
            _runTime = runTime;

            _updateInterval = new TimeSpan(0, 0, 0, 0, 10);

            Tick += Update;
        }

        public Countdown(TimeSpan runTime, TimeSpan updateInterval)
        {
            _runTime = runTime;

            _updateInterval = updateInterval;

            Tick += Update;
        }

        public event EventHandler CountDownFinishedEvent;

        public event Action Tick;
        public bool TimerRunning { get; private set; }

        private int secondsRemaining = 1200;
        public int SecondsRemaining { get => secondsRemaining; set { secondsRemaining = value; OnPropertyChanged(); } }

        public string CurrentTimeString
        {
            get => _currentTimeString;
            set
            {
                _currentTimeString = value;
                OnPropertyChanged();
            }
        }

        public void Start()
        {
            _shouldStop = false;
            _timeToStop = DateTime.Now + _runTime;
            Task task = new(GenerateTicks);
            task.Start();
            TimerRunning = true;
        }

        public void Stop()
        {
            _shouldStop = true;
            TimerRunning = false;
            CurrentTimeString = "00:00";
            SecondsRemaining = 1200;
        }

        private void GenerateTicks()
        {
            while (_shouldStop == false)
            {
                Tick?.Invoke();

                Thread.Sleep(_updateInterval);
            }
        }

        private void Update()
        {
            TimeSpan timeLeft = _timeToStop - DateTime.Now;

            if (timeLeft <= TimeSpan.Zero)
            {
                _shouldStop = true;
                CurrentTimeString = "00:00";
                SecondsRemaining = 1200;
                CountDownFinishedEvent?.Invoke(this, new EventArgs());
                return;
            }

            DateTime timeLeftDate = DateTime.MinValue + timeLeft;

            CurrentTimeString = timeLeftDate.ToString("mm:ss");
            SecondsRemaining = 1200 - (int)timeLeft.TotalSeconds;
        }
    }
}
