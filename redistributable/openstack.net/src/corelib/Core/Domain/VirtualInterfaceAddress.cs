namespace net.openstack.Core.Domain
{
    using System.Net;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the IP address of a virtual interface on a specific network.
    /// </summary>
    /// <remarks>
    /// <note>
    /// Virtual network interfaces are a Rackspace-specific extension to the OpenStack Networking Service.
    /// </note>
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class VirtualInterfaceAddress : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the IP address of the virtual interface.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        [JsonProperty("address")]
        [JsonConverter(typeof(IPAddressSimpleConverter))]
        public IPAddress Address
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ID of the network this virtual interface is connected to.
        /// </summary>
        /// <seealso cref="CloudNetwork.Id"/>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        [JsonProperty("network_id")]
        public string NetworkId { get; private set; }

        /// <summary>
        /// Gets the label of the network this virtual interface is connected to.
        /// </summary>
        /// <seealso cref="CloudNetwork.Label"/>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        [JsonProperty("network_label")]
        public string NetworkLabel { get; private set; }
    }
}