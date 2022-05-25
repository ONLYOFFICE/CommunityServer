namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Net;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a single entry in a load balancer access list.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NetworkItem : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        private NetworkItemId _id;
#pragma warning restore 649

        /// <summary>
        /// This is the backing field for the <see cref="Address"/> property.
        /// </summary>
        [JsonProperty("address")]
        private string _address;

        /// <summary>
        /// This is the backing field for the <see cref="AccessType"/> property.
        /// </summary>
        [JsonProperty("type")]
        private AccessType _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkItem"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NetworkItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkItem"/> class with the
        /// specified IP address and access type.
        /// </summary>
        /// <param name="address">The IP address to which this network item applies.</param>
        /// <param name="accessType">The access type for this network item.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="address"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="accessType"/> is <see langword="null"/>.</para>
        /// </exception>
        public NetworkItem(IPAddress address, AccessType accessType)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (accessType == null)
                throw new ArgumentNullException("accessType");

            _address = address.ToString();
            _type = accessType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkItem"/> class with the
        /// specified IP address and access type.
        /// </summary>
        /// <param name="address">The IP address, or address range in CIDR notation, to which this network item applies.</param>
        /// <param name="accessType">The access type for this network item.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="address"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="accessType"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="address"/> is empty.</exception>
        public NetworkItem(string address, AccessType accessType)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (accessType == null)
                throw new ArgumentNullException("accessType");
            if (string.IsNullOrEmpty(address))
                throw new ArgumentException("address cannot be empty");

            _address = address;
            _type = accessType;
        }

        /// <summary>
        /// Gets the unique ID associated with this network item resource in the load balancer service.
        /// </summary>
        public NetworkItemId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the IP address, or address range in CIDR notation, to which this network item applies.
        /// </summary>
        public string Address
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// The access type for this network item.
        /// </summary>
        public AccessType AccessType
        {
            get
            {
                return _type;
            }
        }
    }
}
