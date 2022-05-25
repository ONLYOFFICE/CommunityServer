using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Defines a set of fields to update on a floating IP.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "floatingip")]
    public class FloatingIPUpdateDefinition
    {
        /// <summary>
        /// The new port to which the floating IP should be associated, or null to disassociate the floating IP.
        /// </summary>
        [JsonProperty("port_id")]
        public Identifier PortId { get; set; }
    }
}