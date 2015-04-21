using System;
using System.Collections.Generic;
using System.Text;

namespace Jurassic
{

    /// <summary>
    /// Contains handy string routines.
    /// </summary>
    internal static class StringHelpers
    {
        /// <summary>
        /// Concatenates a specified separator string between each element of a specified string
        /// array, yielding a single concatenated string.
        /// </summary>
        /// <typeparam name="T"> The type of the members of <paramref name="values"/> </typeparam>
        /// <param name="separator"> The string to use as a separator. </param>
        /// <param name="values"> A collection that contains the objects to concatenate. </param>
        /// <returns> A string that consists of the members of <paramref name="values"/> delimited
        /// by the <paramref name="separator"/> string. </returns>
        internal static string Join<T>(string separator, IEnumerable<T> values)
        {
            if (separator == null)
                throw new ArgumentNullException("separator");
            if (values == null)
                throw new ArgumentNullException("values");
            var result = new StringBuilder();
            bool first = true;
            foreach (object value in values)
            {
                if (first == false)
                    result.Append(separator);
                first = false;
                result.Append(value);
            }
            return result.ToString();
        }
    }

}
