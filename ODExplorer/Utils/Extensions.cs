using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ODExplorer.Utils
{
    public static class Extensions
    {
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                collection.Move(collection.IndexOf(sorted[i]), i);
            }
        }

        public static void SortDataGrid(this DataGrid dataGrid, List<SortDescription> sortDescriptions)
        {
            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();
            // Add the new sort descriptions
            foreach (SortDescription sort in sortDescriptions)
            {
                dataGrid.Items.SortDescriptions.Add(sort);
            }
            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        public static void SortDataGrid(this DataGrid dataGrid, SortDescription sortDescription)
        {
            // Clear current sort descriptions
            dataGrid.Items.SortDescriptions.Clear();
            // Add the new sort descriptions
            dataGrid.Items.SortDescriptions.Add(sortDescription);
            // Refresh items to display sort
            dataGrid.Items.Refresh();
        }

        #region Collection operations on the main ui thread
        /// <summary>
        /// Clears a collection on the main ui thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        public static void ClearCollection<T>(this ObservableCollection<T> collection)
        {
            if (!collection.Any())
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                collection.Clear();
            });
        }
        /// <summary>
        /// Adds a single object to a collection on the main ui thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="objectToAdd"></param>
        public static void AddToCollection<T>(this ObservableCollection<T> collection, object objectToAdd)
        {
            if (objectToAdd is null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                collection.Add((T)objectToAdd);
            });
        }
        /// <summary>
        /// Add a range of a collection to a collection on the main ui thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="collectionToAdd"></param>
        public static void AddRangeToCollection<T>(this ObservableCollection<T> collection, IEnumerable<T> collectionToAdd)
        {
            if (collectionToAdd == null || !collectionToAdd.Any())
            {
                return;
            }

            foreach (var item in collectionToAdd)
            {
                AddToCollection(collection, item);
            }
        }
        /// <summary>
        /// Removes an object from a collection on the main ui thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="objectToRemove"></param>
        public static void RemoveFromCollection<T>(this ObservableCollection<T> collection, object objectToRemove)
        {
            if (objectToRemove is null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                collection.Remove((T)objectToRemove);
            });
        }

        public static void RemoveAtIndexFromCollection<T>(this ObservableCollection<T> collection, int index)
        {
            if (collection == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                collection.RemoveAt(index);
            });
        }
        #endregion

        public static ListSortDirection Reverse(this ListSortDirection sortDirection)
        {
            return sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }
    }
}
