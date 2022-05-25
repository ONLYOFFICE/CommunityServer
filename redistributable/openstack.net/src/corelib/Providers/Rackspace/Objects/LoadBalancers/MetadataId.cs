namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a metadata item in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <seealso cref="LoadBalancerMetadataItem.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(MetadataId.Converter))]
    public sealed class MetadataId : ResourceIdentifier<MetadataId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The metadata item identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public MetadataId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="MetadataId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override MetadataId FromValue(string id)
            {
                return new MetadataId(id);
            }
        }
    }
}
