namespace net.openstack.Core.Domain.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using net.openstack.Core.Collections;
    using net.openstack.Core.Providers;

    /// <summary>
    /// This class extends the <see cref="ReadOnlyCollectionPage{T}"/> class
    /// to provide access to the opaque marker used for paginating messages
    /// in the <see cref="IQueueingService"/> (via the <see cref="NextPageId"/>
    /// property.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public class QueuedMessageList : BasicReadOnlyCollectionPage<QueuedMessage>
    {
        /// <summary>
        /// This is the backing field for the <see cref="NextPageId"/> property.
        /// </summary>
        private QueuedMessageListId _nextPageId;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedMessageList"/> class
        /// that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        /// <param name="getNextPageAsync">A function that returns a <see cref="Task{TResult}"/> representing the asynchronous operation to get the next page of items in the collection. If specified, this function implements <see cref="BasicReadOnlyCollectionPage{T}.GetNextPageAsync"/>. If the value is <see langword="null"/>, then <see cref="BasicReadOnlyCollectionPage{T}.CanHaveNextPage"/> will return <see langword="false"/>.</param>
        /// <param name="nextPageId">The identifier of the next page in the message list.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="list"/> is <see langword="null"/>.
        /// </exception>
        public QueuedMessageList(IList<QueuedMessage> list, Func<CancellationToken, Task<ReadOnlyCollectionPage<QueuedMessage>>> getNextPageAsync, QueuedMessageListId nextPageId)
            : base(list, getNextPageAsync)
        {
            _nextPageId = nextPageId;
        }

        /// <summary>
        /// Gets the identifier of the next page of the message list.
        /// </summary>
        public QueuedMessageListId NextPageId
        {
            get
            {
                return _nextPageId;
            }
        }
    }
}
