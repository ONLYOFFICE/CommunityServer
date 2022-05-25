using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStack
{
    /// <summary>
    /// A page of resources.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    public interface IPage<T> : IEnumerable<T>
    {
        /// <summary>
        /// Specifies if another page can be retrieved with additional items
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Retrieves the next page of items, if one is available. If <see cref="HasNextPage"/> is <c>false</c>, an empty page is returned.
        /// </summary>
        Task<IPage<T>> GetNextPageAsync(CancellationToken cancellation = default(CancellationToken));
    }
}