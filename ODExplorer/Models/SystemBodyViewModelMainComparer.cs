using ODExplorer.ViewModels.ModelVMs;
using System;
using System.Collections;
using System.ComponentModel;

namespace ODExplorer.Models
{
    public sealed class SystemBodyViewModelMainComparer(SystemGridSettings settings) : IComparer
    {
        private readonly SystemGridSettings settings = settings;

        public int Compare(object? x, object? y)
        {
            if (x is not SystemBodyViewModel a || y is not SystemBodyViewModel b)
                throw new ArgumentException("Not a SystemBodyViewModel");

            if (settings.ExcludeStarsFromSorting)
            {
                var isStar = a.IsStar.CompareTo(b.IsStar);
                if (isStar != 0)
                    return isStar;

                if(a.IsStar && b.IsStar)
                {
                    return b.DistanceFromArrival.CompareTo(a.DistanceFromArrival);
                }
            }

            var isEdsmVb = b.IsEdsmVb.CompareTo(a.IsEdsmVb);

            if (isEdsmVb != 0)
                return isEdsmVb;

            var direction = settings.SortDirection == ListSortDirection.Ascending ? 1 : -1;

            var ret = 0;
            switch (settings.BodySortingOptions)
            {
                case BodySortCategory.Gravity:
                    if (direction == 1)
                        return a.SurfaceGravity.CompareTo(b.SurfaceGravity);
                    return b.SurfaceGravity.CompareTo(a.SurfaceGravity);
                case BodySortCategory.Distance:
                    if (direction == 1)
                        return a.DistanceFromArrival.CompareTo(b.DistanceFromArrival);
                    return b.DistanceFromArrival.CompareTo(a.DistanceFromArrival);
                case BodySortCategory.Type:
                    if (direction == 1)
                        return string.Compare(a.PlanetClassDescription, b.PlanetClassDescription, StringComparison.OrdinalIgnoreCase);
                    return string.Compare(b.PlanetClassDescription, a.PlanetClassDescription, StringComparison.OrdinalIgnoreCase);
                case BodySortCategory.Name:
                    if (direction == 1)
                        return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                    return string.Compare(b.Name, a.Name, StringComparison.OrdinalIgnoreCase);
                case BodySortCategory.BodyId:
                    if (direction == 1)
                        return a.BodyID.CompareTo(b.BodyID);
                    return b.BodyID.CompareTo(a.BodyID);
                case BodySortCategory.BioSignals:
                    if (direction == 1)
                        return a.BiologicalSignals.CompareTo(b.BiologicalSignals);
                    return b.BiologicalSignals.CompareTo(a.BiologicalSignals);
                case BodySortCategory.GeoSignals:
                    if (direction == 1)
                        return a.GeologicalSignals.CompareTo(b.GeologicalSignals);
                    return b.GeologicalSignals.CompareTo(a.GeologicalSignals);
                case BodySortCategory.WorthMappingValue:
                    ret = b.WorthMapping.CompareTo(a.WorthMapping);

                    if (ret != 0)
                        return ret;

                    if (direction == 1)
                    {
                        return a.MappedValueActual.CompareTo(b.MappedValueActual);
                    }

                    return b.MappedValueActual.CompareTo(a.MappedValueActual);
                case BodySortCategory.WorthMappingDistance:
                    ret = b.WorthMapping.CompareTo(a.WorthMapping);

                    if (ret != 0)
                        return ret;

                    if (direction == 1)
                    {
                        return a.DistanceFromArrival.CompareTo(b.DistanceFromArrival);
                    }

                    return b.DistanceFromArrival.CompareTo(a.DistanceFromArrival);
                case BodySortCategory.Value:
                    if (direction == 1)
                        return a.MappedValueActual.CompareTo(b.MappedValueActual);
                    return b.MappedValueActual.CompareTo(a.MappedValueActual);
                case BodySortCategory.None:
                    return 0;
                default:
                    if (direction == 1)
                        return a.MappedValueActual.CompareTo(b.MappedValueActual);
                    return b.MappedValueActual.CompareTo(a.MappedValueActual);
            }
        }
    }
}
