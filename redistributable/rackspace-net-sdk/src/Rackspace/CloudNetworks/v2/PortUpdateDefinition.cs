using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace Rackspace.CloudNetworks.v2
{
    /// <summary>
    /// Represents the set of properties which can be modified when updating a <see cref="Port"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "port")]
    public class PortUpdateDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortUpdateDefinition"/> class.
        /// </summary>
        public PortUpdateDefinition()
        {
            FixedIPs = new List<IPAddressAssociation>();
            SecurityGroups = new List<Identifier>();
        }
        
        /// <summary>
        /// The port name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// IP addresses for the port.
        /// </summary>
        [JsonProperty("fixed_ips")]
        public IList<IPAddressAssociation> FixedIPs { get; set; }

        /// <summary>
        /// The IDs of any attached security groups.
        /// </summary>
        [JsonProperty("security_groups")]
        public IList<Identifier> SecurityGroups { get; set; }
    }
}
