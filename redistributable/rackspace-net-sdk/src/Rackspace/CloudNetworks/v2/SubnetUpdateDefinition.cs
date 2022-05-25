using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// Represents the set of properties which can be modified when updating a <see cref="Subnet"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "subnet")]
    public class SubnetUpdateDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubnetUpdateDefinition"/> class.
        /// </summary>
        public SubnetUpdateDefinition()
        {
            Nameservers = new List<string>();
            AllocationPools = new List<AllocationPool>();
            HostRoutes = new List<HostRoute>();
        }

        /// <summary>
        /// The subnet name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The DNS nameserver IP addresses.
        /// </summary>
        [JsonProperty("dns_nameservers")]
        public IList<string> Nameservers { get; set; }

        /// <summary>
        /// The IP address allocation pools.
        /// </summary>
        [JsonProperty("allocation_pools")]
        public IList<AllocationPool> AllocationPools { get; set; }

        /// <summary>
        /// The host routes.
        /// </summary>
        [JsonProperty("host_routes")]
        public List<HostRoute> HostRoutes { get; set; }

        /// <summary>
        /// The gateway IP address.
        /// </summary>
        [JsonProperty("gateway_ip")]
        public string GatewayIP { get; set; }
    }
}