using System;
using System.Collections.Generic;
using System.Linq;

namespace Ical.Net.Utility
{
    internal static class CollectionHelpers
    {
        /// <summary> Commutative, stable, order-independent hashing for collections of collections. </summary>
        public static int GetHashCode<T>(IEnumerable<T> collection)
        {
            unchecked
            {
                if (collection == null)
                {
                    return 0;
                }

                return collection
                    .Where(e => e != null)
                    .Aggregate(0, (current, element) => current + element.GetHashCode());
            }
        }

        /// <summary> Commutative, stable, order-independent hashing for collections of collections. </summary>
        public static int GetHashCodeForNestedCollection<T>(IEnumerable<IEnumerable<T>> nestedCollection)
        {
            unchecked
            {
                if (nestedCollection == null)
                {
                    return 0;
                }

                return nestedCollection
                    .SelectMany(c => c)
                    .Where(e => e != null)
                    .Aggregate(0, (current, element) => current + element.GetHashCode());
            }
        }

        public static bool Equals<T>(IEnumerable<T> left, IEnumerable<T> right, bool orderSignificant = false)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null && right != null)
            {
                return false;
            }

            if (left != null && right == null)
            {
                return false;
            }

            if (orderSignificant)
            {
                return left.SequenceEqual(right);
            }

            try
            {
                //Many things have natural IComparers defined, but some don't, because no natural comparer exists
                return left.OrderBy(l => l).SequenceEqual(right.OrderBy(r => r));
            }
            catch (Exception)
            {
                //It's not possible to sort some collections of things (like Calendars) in any meaningful way. Properties can be null, and there's no natural
                //ordering for the contents therein. In cases like that, the best we can do is treat them like sets, and compare them. We don't maintain
                //fidelity with respect to duplicates, but it seems better than doing nothing
                var leftSet = new HashSet<T>(left);
                var rightSet = new HashSet<T>(right);
                return leftSet.SetEquals(rightSet);
            }
        }

        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (var element in source)
            {
                destination.Add(element);
            }
        }
    }
}
