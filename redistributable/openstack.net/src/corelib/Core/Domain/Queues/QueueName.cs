namespace net.openstack.Core.Domain.Queues
{
    using System;
    using Newtonsoft.Json;
    using net.openstack.Core.Providers;

    /// <summary>
    /// Represents the name of a queue in the <see cref="IQueueingService"/>.
    /// </summary>
    /// <seealso cref="CloudQueue.Name"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(QueueName.Converter))]
    public sealed class QueueName : ResourceIdentifier<QueueName>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueName"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The queue name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public QueueName(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="QueueName"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override QueueName FromValue(string id)
            {
                return new QueueName(id);
            }
        }
    }
}
