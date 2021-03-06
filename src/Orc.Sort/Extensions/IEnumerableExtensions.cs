﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orc.Sort.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Takes a collection of sorted items and merges these collections together in order to return one sorted collection.
        /// </summary>
        /// <typeparam name="T"> Must be comparable. </typeparam>
        /// <param name="sortedEnumerables"></param>
        /// <param name="itemComparer"></param>
        /// <returns></returns>
        public static IEnumerable<T> MergeSorted<T>(this IEnumerable<IEnumerable<T>> sortedEnumerables)
            where T : IComparable<T>
        {
            return sortedEnumerables.MergeSorted(Comparer<T>.Default);
        }

        public static IEnumerable<T> MergeSorted<T>(this IEnumerable<IEnumerable<T>> sortedEnumerables, IComparer<T> itemComparer)
        {
            var sortedKeySet = new SortedSet<KeyedEnumerator<T>>();
            int secondaryKey = 0;

            foreach (var enumerator in sortedEnumerables.Select(e => e.GetEnumerator()))
            {
                if (enumerator.MoveNext())
                {
                    sortedKeySet.Add(new KeyedEnumerator<T>(enumerator, itemComparer, secondaryKey++));
                }
            }

            while (sortedKeySet.Count > 1)
            {
                var minItem = sortedKeySet.Min;
                var hasNext = true;
                sortedKeySet.Remove(minItem);

                while (hasNext && minItem.CompareTo(sortedKeySet.Min) < 0)
                {
                    yield return minItem.Current;
                    hasNext = minItem.MoveNext();
                }

                if (hasNext)
                {
                    sortedKeySet.Add(minItem);
                }
            }

            if (sortedKeySet.Count > 0)
            {
                var min = sortedKeySet.Min;
                var has = true;
                sortedKeySet.Remove(min);

                while (has)
                {
                    yield return min.Current;
                    has = min.MoveNext();
                }
            }

        }
    }

    internal class KeyedEnumerator<T> : IEnumerator<T>, IComparable<KeyedEnumerator<T>>
    {
        public KeyedEnumerator(IEnumerator<T> enumerator, IComparer<T> itemComparer, int secondaryKey)
        {
            Enumerator = enumerator;
            ItemComparer = itemComparer;
            SecondaryKey = secondaryKey;
        }

        public int CompareTo(KeyedEnumerator<T> other)
        {
            var result = ItemComparer.Compare(Current, other.Current);

            if (result == 0)
            {
                result = SecondaryKey.CompareTo(other.SecondaryKey);
            }

            return result;
        }

        public void Dispose()
        {
            Enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return Enumerator.MoveNext();
        }

        public void Reset()
        {
            Enumerator.Reset();
        }

        public T Current
        {
            get { return Enumerator.Current; }
        }

        object IEnumerator.Current
        {
            get { return Enumerator.Current; }
        }

        public IEnumerator<T> Enumerator { get; private set; }

        public IComparer<T> ItemComparer { get; private set; }

        public int SecondaryKey { get; private set; }

    }
}
