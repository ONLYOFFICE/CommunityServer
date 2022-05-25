namespace net.openstack.Core.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides the base class for a generic read-only collection representing a
    /// single page within a paginated collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public abstract class ReadOnlyCollectionPage<T> : ReadOnlyCollection<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyCollectionPage{T}"/> class
        /// that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="list"/> is <see langword="null"/>.</exception>
        protected ReadOnlyCollectionPage(IList<T> list)
            : base(list)
        {
        }

        /// <summary>
        /// Gets an empty page from a paginated collection.
        /// </summary>
        public static ReadOnlyCollectionPage<T> Empty
        {
            get
            {
                return EmptyPage.Instance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether another page of results may follow the current page.
        /// </summary>
        /// <value><see langword="true"/> if another page of results may follow the current page; otherwise, <see langword="false"/>.</value>
        public abstract bool CanHaveNextPage
        {
            get;
        }

        /// <summary>
        /// Gets the next page in the paginated collection.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that the task will observe.</param>
        /// <returns>
        /// A <see cref="Task"/> object representing the asynchronous operation. When the task
        /// completes successfully, the <see cref="Task{TResult}.Result"/> property will contain
        /// a collection containing the next page of results.
        /// </returns>
        /// <exception cref="InvalidOperationException">If <see cref="CanHaveNextPage"/> is <see langword="false"/>.</exception>
        /// <exception cref="WebException">If the REST request does not return successfully.</exception>
        public abstract Task<ReadOnlyCollectionPage<T>> GetNextPageAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Represents an empty page from a paginated collection.
        /// </summary>
        private sealed class EmptyPage : ReadOnlyCollectionPage<T>
        {
            /// <summary>
            /// This is the backing field for the <see cref="Instance"/> property.
            /// </summary>
            private static readonly EmptyPage _instance = new EmptyPage();

            /// <summary>
            /// Initializes a new instance of the <see cref="EmptyPage"/> class.
            /// </summary>
            private EmptyPage()
                : base(new T[0])
            {
            }

            /// <summary>
            /// Gets a singleton instance of an empty page from a paginated collection.
            /// </summary>
            public static EmptyPage Instance
            {
                get
                {
                    return _instance;
                }
            }

            /// <inheritdoc/>
            public override bool CanHaveNextPage
            {
                get
                {
                    return false;
                }
            }

            /// <inheritdoc/>
            public override Task<ReadOnlyCollectionPage<T>> GetNextPageAsync(CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Cannot obtain the next page when CanHaveNextPage is false.");
            }
        }
    }
}
