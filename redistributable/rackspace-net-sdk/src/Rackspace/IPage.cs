using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenStack.Synchronous.Extensions;

namespace Rackspace
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

namespace Rackspace.Synchronous
{
    /// <summary>
    /// Provides synchronous extention methods for an <see cref="IPage{T}"/> instance.
    /// </summary>
    public static class PageExtensions
    {
        /// <summary>
        /// Retrieves the next page of items, if one is available. If <see cref="IPage{T}.HasNextPage"/> is <c>false</c>, an empty page is returned.
        /// </summary>
        public static IPage<T> GetNextPage<T>(this IPage<T> page)
        {
            return page.GetNextPageAsync().ForceSynchronous();
        }
    }
}
