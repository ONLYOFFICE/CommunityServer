using Newtonsoft.Json;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines additional networks to which a server should be attached.
    /// </summary>
    public class ServerNetworkDefinition
    {
        /// <summary>
        /// Provisions a server with a NIC for the specified network. Required if you omit <see cref="PortId"/>.
        /// </summary>
        [JsonProperty("uuid")]
        public Identifier NetworkId { get; set; }

        /// <summary>
        /// Provisions a server with a NIC for the specified port. Required if you omit <see cref="NetworkId"/>. 
        /// </summary>
        [JsonProperty("port")]
        public Identifier PortId { get; set; }

        /// <summary>
        /// A fixed IPv4 address for the NIC.
        /// </summary>
        [JsonProperty("fixed_ip")]
        public string IPAddress { get; set; }
    }
}