namespace net.openstack.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a basic implementation of <see cref="ReadOnlyCollectionPage{T}"/> using
    /// a function delegate as the implementation of <see cref="GetNextPageAsync"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class BasicReadOnlyCollectionPage<T> : ReadOnlyCollectionPage<T>
    {
        /// <summary>
        /// This is the backing field for both <see cref="CanHaveNextPage"/> and <see cref="GetNextPageAsync"/>.
        /// </summary>
        private readonly Func<CancellationToken, Task<ReadOnlyCollectionPage<T>>> _getNextPageAsync;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicReadOnlyCollectionPage{T}"/> class
        /// that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <param name="getNextPageAsync">A function that returns a <see cref="Task{TResult}"/> representing the asynchronous operation to get the next page of items in the collection. If specified, this function implements <see cref="GetNextPageAsync"/>. If the value is <see langword="null"/>, then <see cref="CanHaveNextPage"/> will return <see langword="false"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="list"/> is <see langword="null"/>.
        /// </exception>
        public BasicReadOnlyCollectionPage(IList<T> list, Func<CancellationToken, Task<ReadOnlyCollectionPage<T>>> getNextPageAsync)
            : base(list)
        {
            _getNextPageAsync = getNextPageAsync;
        }

        /// <inheritdoc/>
        public override bool CanHaveNextPage
        {
            get
            {
                return _getNextPageAsync != null;
            }
        }

        /// <inheritdoc/>
        public override Task<ReadOnlyCollectionPage<T>> GetNextPageAsync(CancellationToken cancellationToken)
        {
            if (!CanHaveNextPage)
                throw new InvalidOperationException("Cannot obtain the next page when CanHaveNextPage is false.");

            return _getNextPageAsync(cancellationToken);
        }
    }
}
