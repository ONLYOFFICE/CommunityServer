namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an access control network item in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <seealso cref="NetworkItem.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NetworkItemId.Converter))]
    public sealed class NetworkItemId : ResourceIdentifier<NetworkItemId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkItemId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The network item identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public NetworkItemId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NetworkItemId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NetworkItemId FromValue(string id)
            {
                return new NetworkItemId(id);
            }
        }
    }
}
