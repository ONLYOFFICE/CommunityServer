using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Networking.v2.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Represents the set of properties which can be initialized when creating a <see cref="Port"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class PortCreateDefinition : PortUpdateDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortCreateDefinition"/> class.
        /// </summary>
        protected PortCreateDefinition()
        {
            AllowedAddresses = new List<AllowedAddress>();
            DHCPOptions = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortCreateDefinition"/> class.
        /// </summary>
        /// <param name="networkId">The network identifier.</param>
        public PortCreateDefinition(Identifier networkId) : this()
        {
            NetworkId = networkId;
        }
        
        /// <summary>
        /// The ID of the attached network.
        /// </summary>
        [JsonProperty("network_id")]
        public Identifier NetworkId { get; set; }
        
        /// <summary>
        /// Allowed address pairs.
        /// </summary>
        [JsonProperty("allowed_address_pairs")]
        public IList<AllowedAddress> AllowedAddresses { get; set; }
        
        /// <summary>
        /// The MAC address.
        /// </summary>
        [JsonProperty("mac_address")]
        public string MACAddress { get; set; }
        
        /// <summary>
        /// Additional DHCP options.
        /// </summary>
        /// <seealso href="http://specs.openstack.org/openstack/neutron-specs/specs/api/extra_dhcp_options__extra-dhcp-opt_.html"/>
        [JsonProperty("extra_dhcp_opts")]
        [JsonConverter(typeof(DHCPOptionsConverter))]
        public IDictionary<string, string> DHCPOptions { get; set; }

        /// <summary>
        /// The ID of the entity that uses this port. For example, a DHCP agent.
        /// </summary>
        [JsonProperty("device_owner")]
        public string DeviceOwner { get; set; }

        /// <summary>
        /// The ID of the device that uses this port. For example, a virtual server.
        /// </summary>
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
    }
}
