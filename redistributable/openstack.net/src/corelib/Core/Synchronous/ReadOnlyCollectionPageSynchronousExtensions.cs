namespace net.openstack.Core.Synchronous
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading;
    using net.openstack.Core.Collections;

    /// <summary>
    /// Provides extension methods to allow synchronous calls to the methods in <see cref="ReadOnlyCollectionPage{T}"/>,
    /// along with the asynchronous extension methods for that class defined in <see cref="ReadOnlyCollectionPageExtensions"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
    public static class ReadOnlyCollectionPageSynchronousExtensions
    {
        /// <summary>
        /// Get all pages in a paginated collection.
        /// </summary>
        /// <remarks>
        /// This method determines that the end of the collection is reached when either of
        /// the following conditions is true.
        /// <list type="bullet">
        /// <item>The <see cref="ReadOnlyCollectionPage{T}.CanHaveNextPage"/> property returns <see langword="false"/>.</item>
        /// <item>An empty page is reached.</item>
        /// </list>
        /// </remarks>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="page">The first page in the collection.</param>
        /// <returns>A read-only collection containing the complete set of results from the paginated collection.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="page"/> is <see langword="null"/>.</exception>
        /// <seealso cref="ReadOnlyCollectionPageExtensions.GetAllPagesAsync"/>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollection<T> GetAllPages<T>(this ReadOnlyCollectionPage<T> page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            try
            {
                return page.GetAllPagesAsync(CancellationToken.None, null).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }

        /// <summary>
        /// Gets the next page in the paginated collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="page">A page of a paginated collection.</param>
        /// <returns>A collection containing the next page of results.</returns>
        /// <exception cref="InvalidOperationException">If <see cref="ReadOnlyCollectionPage{T}.CanHaveNextPage"/> is <see langword="false"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        /// <seealso cref="ReadOnlyCollectionPage{T}.GetNextPageAsync"/>
        [Obsolete("These synchronous wrappers should not be used. For more information, see http://blogs.msdn.com/b/pfxteam/archive/2012/04/13/10293638.aspx.")]
        public static ReadOnlyCollectionPage<T> GetNextPage<T>(this ReadOnlyCollectionPage<T> page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            try
            {
                return page.GetNextPageAsync(CancellationToken.None).Result;
            }
            catch (AggregateException ex)
            {
                ReadOnlyCollection<Exception> innerExceptions = ex.Flatten().InnerExceptions;
                if (innerExceptions.Count == 1)
                    throw innerExceptions[0];

                throw;
            }
        }
    }
}
