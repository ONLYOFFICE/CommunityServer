namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a virtual address in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <seealso cref="LoadBalancerVirtualAddress.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(VirtualAddressId.Converter))]
    public sealed class VirtualAddressId : ResourceIdentifier<VirtualAddressId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualAddressId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The virtual address identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public VirtualAddressId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="VirtualAddressId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override VirtualAddressId FromValue(string id)
            {
                return new VirtualAddressId(id);
            }
        }
    }
}
