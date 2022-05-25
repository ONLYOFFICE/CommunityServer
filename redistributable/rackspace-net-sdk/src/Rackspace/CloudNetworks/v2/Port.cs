using Newtonsoft.Json;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// Represents a port resource of the <see cref="CloudNetworkService"/>.
    /// <para/>
    /// <para>
    /// Virtual (or logical) switch ports on a given network.
    /// </para>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class Port : PortCreateDefinition
    {
        /// <summary>
        /// The ID of the port.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }
        
        /// <summary>
        /// The port status.
        /// </summary>
        [JsonProperty("status")]
        public PortStatus Status { get; set; }

        /// <summary>
        /// The administrative state of the port.
        /// </summary>
        [JsonProperty("admin_state_up")]
        public bool IsUp { get; set; }

        /// <summary>
        /// The MAC address.
        /// </summary>
        [JsonProperty("mac_address")]
        public string MACAddress { get; set; }
    }
}
