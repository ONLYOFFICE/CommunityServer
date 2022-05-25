namespace net.openstack.Core.Domain.Queues
{
    using System;
    using Newtonsoft.Json;
    using net.openstack.Core.Providers;

    /// <summary>
    /// Represents the unique identifier of a message in the <see cref="IQueueingService"/>.
    /// </summary>
    /// <seealso cref="QueuedMessage.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(MessageId.Converter))]
    public sealed class MessageId : ResourceIdentifier<MessageId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The message identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public MessageId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="MessageId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override MessageId FromValue(string id)
            {
                return new MessageId(id);
            }
        }
    }
}
