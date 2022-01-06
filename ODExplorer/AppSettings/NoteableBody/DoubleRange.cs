using ODExplorer.Utils;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ODExplorer.AppSettings.NoteableBody
{
    public class DoubleRange : PropertyChangeNotify
    {
        public DoubleRange(double defaultMin, double defaultMax)
        {
            DefaultMin = defaultMin;
            DefaultMax = defaultMax;
            ResetValues();
        }
        [IgnoreDataMember]
        public double DefaultMin { get; private set; }
        [IgnoreDataMember]
        public double DefaultMax { get; private set; }

        private double minimum;
        public double Minimun { get => minimum; set { minimum = value; OnPropertyChanged(); } }

        private double maximum;
        public double Maximum { get => maximum; set { maximum = value; OnPropertyChanged(); } }

        private bool isActive;
        public bool IsActive { get => isActive; set { isActive = value; OnPropertyChanged(); } }

        public void CheckInRange(List<bool> bools, double valueToCheck)
        {
            if (isActive == false)
            {
                return;
            }

            bools.Add(valueToCheck >= minimum && valueToCheck <= maximum);
        }

        public void ResetValues()
        {
            minimum = DefaultMin;
            maximum = DefaultMax;
            isActive = false;
        }

        public void SetValues(DoubleRange doubleRange)
        {
            Minimun = doubleRange.Minimun;
            Maximum = doubleRange.Maximum;
            IsActive = doubleRange.IsActive;
        }
    }
}
