namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;
    using CloudNetwork = net.openstack.Core.Domain.CloudNetwork;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;
    using NetworkId = net.openstack.Core.Domain.NetworkId;

    /// <summary>
    /// This class models the JSON representation of a network to initially connect to new
    /// servers created by a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="ServerArgument.Networks"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerNetworkArgument : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="NetworkId"/> property.
        /// </summary>
        [JsonProperty("uuid")]
        private NetworkId _uuid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerNetworkArgument"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ServerNetworkArgument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerNetworkArgument"/> class
        /// with the specified network ID.
        /// </summary>
        /// <param name="networkId">The network ID.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="networkId"/> is <see langword="null"/>.</exception>
        public ServerNetworkArgument(NetworkId networkId)
        {
            if (networkId == null)
                throw new ArgumentNullException("networkId");

            _uuid = networkId;
        }

        /// <summary>
        /// Gets the ID of the network to initially connect servers to.
        /// </summary>
        /// <seealso cref="CloudNetwork.Id"/>
        public NetworkId NetworkId
        {
            get
            {
                return _uuid;
            }
        }
    }
}
