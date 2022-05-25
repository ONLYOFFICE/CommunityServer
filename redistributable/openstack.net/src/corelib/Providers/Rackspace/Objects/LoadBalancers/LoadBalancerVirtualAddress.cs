namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a virtual address which is assigned to a load balancer,
    /// or a virtual address configuration for instructing the load balancer service
    /// to assign a particular type of virtual address to a load balancer.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerVirtualAddress : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Address"/> property.
        /// </summary>
        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(IPAddressSimpleConverter))]
        private IPAddress _address;

        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private VirtualAddressId _id;
#pragma warning restore 649

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancerVirtualAddressType _type;

        /// <summary>
        /// This is the backing field for the <see cref="IPVersion"/> property.
        /// </summary>
        [JsonProperty("ipVersion", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _ipVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerVirtualAddress"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerVirtualAddress()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerVirtualAddress"/> class
        /// with the specified virtual address type.
        /// </summary>
        /// <param name="type">The virtual address type.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <see langword="null"/>.</exception>
        public LoadBalancerVirtualAddress(LoadBalancerVirtualAddressType type)
            : this(type, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerVirtualAddress"/> class
        /// with the specified virtual address type and family.
        /// </summary>
        /// <param name="type">The virtual address type.</param>
        /// <param name="version">The address family for this virtual address, or <see langword="null"/> to not specify the address family.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">If <paramref name="version"/> is not <see cref="AddressFamily.InterNetwork"/> or <see cref="AddressFamily.InterNetworkV6"/>.</exception>
        public LoadBalancerVirtualAddress(LoadBalancerVirtualAddressType type, AddressFamily? version)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _type = type;
            if (version.HasValue)
            {
                switch (version)
                {
                case AddressFamily.InterNetwork:
                    _ipVersion = "IPV4";
                    break;

                case AddressFamily.InterNetworkV6:
                    _ipVersion = "IPV6";
                    break;

                default:
                    throw new NotSupportedException("The specified address family is not supported by this service.");
                }
            }
        }

        /// <summary>
        /// Gets the unique ID representing this virtual address within the load balancers service.
        /// </summary>
        /// <value>
        /// The unique ID for the virtual address, or <see langword="null"/> if the virtual address has not been
        /// created or the JSON response from the server did not include this property.
        /// </value>
        public VirtualAddressId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the IP address for this virtual address within the load balancers service.
        /// </summary>
        /// <value>
        /// The IP address for the virtual address, or <see langword="null"/> if the virtual address has not been
        /// created or the JSON response from the server did not include this property.
        /// </value>
        public IPAddress Address
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// Gets the virtual address type.
        /// </summary>
        /// <value>
        /// A <see cref="LoadBalancerVirtualAddressType"/> object describing the virtual address type,
        /// or <see langword="null"/> if the JSON response from the server did not include this property.
        /// </value>
        public LoadBalancerVirtualAddressType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the address family for this virtual address.
        /// </summary>
        /// <value>
        /// An <see cref="AddressFamily"/> describing the address family for this virtual address,
        /// or <see langword="null"/> if the JSON response from the server did not include this property or
        /// the value was unrecognized.
        /// </value>
        public AddressFamily? IPVersion
        {
            get
            {
                if (_ipVersion == null)
                    return null;

                switch (_ipVersion.ToLowerInvariant())
                {
                case "ipv4":
                    return AddressFamily.InterNetwork;
                case "ipv6":
                    return AddressFamily.InterNetworkV6;
                default:
                    return null;
                }
            }
        }
    }
}
