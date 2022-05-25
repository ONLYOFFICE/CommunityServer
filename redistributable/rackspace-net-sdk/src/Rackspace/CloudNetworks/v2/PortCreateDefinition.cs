using Newtonsoft.Json;

namespace Rackspace.CloudNetworks.v2
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
