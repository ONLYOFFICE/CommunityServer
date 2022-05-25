namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Create Virtual Interface request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/api_create_virtual_interface.html">Create Virtual Interface (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateVirtualInterfaceRequest
    {
        /// <summary>
        /// Gets additional details about the virtual interface to create.
        /// </summary>
        [JsonProperty("virtual_interface")]
        public CreateVirtualInterface VirtualInterface { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateVirtualInterfaceRequest"/>
        /// class with the specified network ID.
        /// </summary>
        /// <param name="networkId">The network ID. This is obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="networkId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="networkId"/> is empty.</exception>
        public CreateVirtualInterfaceRequest(string networkId)
        {
            if (networkId == null)
                throw new ArgumentNullException("networkId");
            if (string.IsNullOrEmpty(networkId))
                throw new ArgumentException("networkId cannot be empty");

            VirtualInterface = new CreateVirtualInterface(networkId);
        }

        /// <summary>
        /// This models the JSON body containing details for a Create Virtual Interface request.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        [JsonObject(MemberSerialization.OptIn)]
        internal class CreateVirtualInterface
        {
            /// <summary>
            /// Gets the network ID.
            /// </summary>
            /// <seealso cref="CloudNetwork.Id"/>
            [JsonProperty("network_id")]
            public string NetworkId { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CreateVirtualInterface"/>
            /// class with the specified network ID.
            /// </summary>
            /// <param name="networkId">The network ID. This is obtained from <see cref="CloudNetwork.Id">CloudNetwork.Id</see>.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="networkId"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">If <paramref name="networkId"/> is empty.</exception>
            public CreateVirtualInterface(string networkId)
            {
                if (networkId == null)
                    throw new ArgumentNullException("networkId");
                if (string.IsNullOrEmpty(networkId))
                    throw new ArgumentException("networkId cannot be empty");

                NetworkId = networkId;
            }
        }
    }
}
