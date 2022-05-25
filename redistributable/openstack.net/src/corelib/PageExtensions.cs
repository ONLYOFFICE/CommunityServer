using OpenStack.Synchronous.Extensions;

namespace OpenStack.Synchronous
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