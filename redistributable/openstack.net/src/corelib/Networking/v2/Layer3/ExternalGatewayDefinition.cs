using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Defines the set of fields that can be updated on an external gateway resource.
    /// </summary>
    public class ExternalGatewayDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalGateway"/> class.
        /// </summary>
        public ExternalGatewayDefinition(Identifier externalNetworkId)
        {
            ExternalNetworkId = externalNetworkId;
            ExternalFixedIPs = new List<IPAddressAssociation>();
        }

        /// <summary>
        /// The external network identifier.
        /// </summary>
        [JsonProperty("network_id")]
        public Identifier ExternalNetworkId { get; set; }

        /// <summary>
        /// Specifies if source NAT is enabled
        /// </summary>
        [JsonProperty("enable_snat")]
        public bool? IsSourceNATEnabled { get; set; }

        /// <summary>
        /// External fixed IP addresses assigned to the router.
        /// </summary>
        [JsonProperty("external_fixed_ips")]
        public IList<IPAddressAssociation> ExternalFixedIPs { get; set; }
    }
}