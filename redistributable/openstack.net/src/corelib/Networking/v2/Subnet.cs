using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Represents a subnet resource of the <see cref="NetworkingService"/>.
    /// <para>
    /// IPv4 or IPv6 address blocks from which IPs to be assigned to VMs on a given network are selected.
    /// </para>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "subnet")]
    public class Subnet : SubnetCreateDefinition
    {
        /// <summary>
        /// The ID of the subnet.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }
        
        /// <summary>
        /// The ID of the tenant who owns the network.
        /// </summary>
        [JsonProperty("tenant_id")]
        public string TenantId { get; set; }

        /// <summary>
        /// Specifies if DHCP is enabled.
        /// </summary>
        [JsonProperty("enable_dhcp")]
        public new bool IsDHCPEnabled { get; set; }
    }
}
