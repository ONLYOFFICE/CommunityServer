namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a domain record in the <see cref="IDnsService"/>.
    /// </summary>
    /// <seealso cref="DnsRecord.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(RecordId.Converter))]
    public sealed class RecordId : ResourceIdentifier<RecordId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The domain record identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public RecordId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="RecordId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override RecordId FromValue(string id)
            {
                return new RecordId(id);
            }
        }
    }
}
