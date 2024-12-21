using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ODExplorer.Extensions
{
    public static class CollectionExtentions
    {
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
        public static void AddToCollection<T>(this ObservableCollection<T> collection, T objectToAdd)
        {
            if (objectToAdd is null)
            {
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                collection.Add(objectToAdd);
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

            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (T item in collectionToAdd)
                {
                    collection.Add(item);
                }
            });
        }
        /// <summary>
        /// Removes an object from a collection on the main ui thread
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="objectToRemove"></param>
        public static void RemoveFromCollection<T>(this ObservableCollection<T> collection, T objectToRemove)
        {
            if (objectToRemove is null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                collection.Remove(objectToRemove);
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

        public static void RemoveAllBeforeItem<T>(this ObservableCollection<T> collection, T item, bool removeItem = false)
        {
            if (collection == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                int index = collection.IndexOf(item) - (removeItem ? 0 : 1);

                if (index < 0)
                {
                    return;
                }

                for (int i = index; i > -1; i--)
                {
                    collection.RemoveAt(i);
                }
            });
        }

        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                List<T> sorted = [.. collection.OrderBy(x => x)];
                for (int i = 0; i < sorted.Count; i++)
                {
                    collection.Move(collection.IndexOf(sorted[i]), i);
                }
            });
        }
        #endregion
    }
}
