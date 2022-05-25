namespace net.openstack.Core.Domain.Queues
{
    using System;
    using net.openstack.Core;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a message list page in the <see cref="IQueueingService"/>.
    /// </summary>
    /// <seealso cref="IQueueingService.ListMessagesAsync"/>
    /// <seealso cref="QueuedMessageList.NextPageId"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(QueuedMessageListId.Converter))]
    public sealed class QueuedMessageListId : ResourceIdentifier<QueuedMessageListId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedMessageListId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public QueuedMessageListId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="QueuedMessageListId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override QueuedMessageListId FromValue(string id)
            {
                return new QueuedMessageListId(id);
            }
        }
    }
}
