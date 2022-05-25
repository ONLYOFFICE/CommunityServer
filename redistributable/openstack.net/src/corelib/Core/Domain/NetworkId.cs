namespace net.openstack.Core.Domain
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a network.
    /// </summary>
    /// <seealso cref="CloudNetwork.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NetworkId.Converter))]
    public sealed class NetworkId : ResourceIdentifier<NetworkId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The network identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public NetworkId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NetworkId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NetworkId FromValue(string id)
            {
                return new NetworkId(id);
            }
        }
    }
}
