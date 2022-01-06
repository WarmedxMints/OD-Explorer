using ODExplorer.Utils;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ODExplorer.AppSettings.NoteableBody
{
    public class IntRange : PropertyChangeNotify
    {
        public IntRange(int defaultMin, int defaultMax)
        {
            DefaultMin = defaultMin;
            DefaultMax = defaultMax;
            ResetValues();
        }
        [IgnoreDataMember]
        public int DefaultMin { get; private set; }
        [IgnoreDataMember]
        public int DefaultMax { get; private set; }

        private int minimum;
        public int Minimun { get => minimum; set { minimum = value; OnPropertyChanged(); } }

        private int maximum;
        public int Maximum { get => maximum; set { maximum = value; OnPropertyChanged(); } }

        private bool isActive;
        public bool IsActive { get => isActive; set { isActive = value; OnPropertyChanged(); } }

        public void CheckInRange(List<bool> bools, int valueToCheck)
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

        public void SetValues(IntRange intRange)
        {
            Minimun = intRange.Minimun;
            Maximum = intRange.Maximum;
            IsActive = intRange.IsActive;
        }
    }
}
