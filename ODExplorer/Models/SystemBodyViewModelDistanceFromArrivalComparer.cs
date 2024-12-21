using ODExplorer.ViewModels.ModelVMs;
using System;
using System.Collections;

namespace ODExplorer.Models
{
    public sealed class SystemBodyViewModelDistanceFromArrivalComparer(bool ascending) : IComparer
    {
        private readonly bool ascending = ascending;

        public int Compare(object? x, object? y)
        {
            if (x is not SystemBodyViewModel a || y is not SystemBodyViewModel b)
                throw new ArgumentException("Not a SystemBodyViewModel");

            if (ascending)
                return a.DistanceFromArrival.CompareTo(b.DistanceFromArrival);

            return b.DistanceFromArrival.CompareTo(a.DistanceFromArrival);
        }
    }
}
