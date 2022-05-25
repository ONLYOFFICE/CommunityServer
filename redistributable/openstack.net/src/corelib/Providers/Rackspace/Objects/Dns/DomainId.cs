namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a domain in the <see cref="IDnsService"/>.
    /// </summary>
    /// <seealso cref="DnsDomain.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DomainId.Converter))]
    public sealed class DomainId : ResourceIdentifier<DomainId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The domain identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public DomainId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DomainId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DomainId FromValue(string id)
            {
                return new DomainId(id);
            }
        }
    }
}
