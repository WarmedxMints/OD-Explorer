using System;
using System.Collections.Generic;
using System.Windows;

namespace ODExplorer.Models
{
    public sealed class GridSize
    {
        public GridLength this[int index]
        {
            get
            {
                if (index < 0 || GridLengths is null || GridLengths.Count - 1 < index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return GridLengths[index];
            }
            set
            {
                if (index < 0 || GridLengths is null || GridLengths.Count - 1 < index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                GridLengths[index] = value;
            }
        }

        public List<GridLength>? GridLengths { get; set; }
    }
}
