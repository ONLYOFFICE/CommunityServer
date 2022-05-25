using System;
using Newtonsoft.Json.Linq;

namespace net.openstack.Core.Domain.Mapping
{
    /// <summary>
    /// Represents an object that can convert between generic <see cref="JObject"/> instances
    /// and instances of another specific type.
    /// </summary>
    /// <typeparam name="T">The type which can be converted to and from <see cref="JObject"/>.</typeparam>
    public interface IJsonObjectMapper<T> : IObjectMapper<JObject, T>
    {
        /// <summary>
        /// Converts a JSON string representation of <typeparamref name="T"/> to an instance
        /// of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="rawJson">The JSON string representation.</param>
        /// <returns>An instance of <typeparamref name="T"/> represented by <paramref name="rawJson"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rawJson"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">If the conversion cannot be performed.</exception>
        T Map(string rawJson);
    }
}
