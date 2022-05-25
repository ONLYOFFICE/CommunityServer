namespace net.openstack.Core.Domain
{
    using System.Net.NetworkInformation;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the detailed configuration of a virtual network interface.
    /// </summary>
    /// <remarks>
    /// <note>
    /// Virtual network interfaces are a Rackspace-specific extension to the OpenStack Networking Service.
    /// </note>
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class VirtualInterface : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the virtual interface ID.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the network addresses associated with the virtual interface.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        [JsonProperty("ip_addresses")]
        public VirtualInterfaceAddress[] Addresses { get; private set; }

        /// <summary>
        /// Gets the Media Access Control (MAC) address for the virtual interface.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/networks/api/v2/cn-devguide/content/list_virt_interfaces.html">List Virtual Interfaces (Rackspace Cloud Networks Developer Guide - OpenStack Networking API v2)</seealso>
        [JsonProperty("mac_address")]
        [JsonConverter(typeof(PhysicalAddressSimpleConverter))]
        public PhysicalAddress MACAddress
        {
            get;
            private set;
        }
    }
}
