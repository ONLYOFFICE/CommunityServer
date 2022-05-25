using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Defines a new floating IP instance.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "floatingip")]
    public class FloatingIPCreateDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FloatingIPCreateDefinition"/> class.
        /// </summary>
        /// <param name="networkId">The network identifier.</param>
        public FloatingIPCreateDefinition(Identifier networkId)
        {
            NetworkId = networkId;
        }

        /// <inheritdoc cref="FloatingIP.NetworkId"/>
        [JsonProperty("floating_network_id")]
        public Identifier NetworkId { get; set; }

        /// <inheritdoc cref="FloatingIP.FixedIPAddress"/>
        [JsonProperty("fixed_ip_address")]
        public string FixedIPAddress { get; set; }

        /// <inheritdoc cref="FloatingIP.FloatingIPAddress"/>
        [JsonProperty("floating_ip_address")]
        public string FloatingIPAddress { get; set; }

        /// <inheritdoc cref="FloatingIP.PortId"/>
        [JsonProperty("port_id")]
        public Identifier PortId { get; set; }
    }
}