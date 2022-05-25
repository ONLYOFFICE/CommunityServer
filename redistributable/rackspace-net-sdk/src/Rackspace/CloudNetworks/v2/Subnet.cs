using Newtonsoft.Json;
using OpenStack.Serialization;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// <para>Represents a subnet resource of the <see cref="CloudNetworkService"/>.</para>
    /// <para/>
    /// <para>IPv4 or IPv6 address blocks from which IPs to be assigned to VMs on a given network are selected.</para>
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
    }
}
