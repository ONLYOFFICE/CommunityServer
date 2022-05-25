namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a transaction in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(TransactionId.Converter))]
    public sealed class TransactionId : ResourceIdentifier<TransactionId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The transaction identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public TransactionId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="TransactionId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override TransactionId FromValue(string id)
            {
                return new TransactionId(id);
            }
        }
    }
}
