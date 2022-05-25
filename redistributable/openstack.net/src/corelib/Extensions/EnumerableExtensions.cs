using System.Linq;

namespace System.Collections.Generic
{
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Creates a new list wrapping the specified items.
        /// <para>If <paramref name="items"/> is <see langword="null" />, returns an empty list.</para>
        /// </summary>
        public static IList<T> ToNonNullList<T>(this IEnumerable<T> items)
        {
            if(items == null)
                return new List<T>();

            return items.ToList();
        }
    }
}
