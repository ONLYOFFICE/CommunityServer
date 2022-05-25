using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Defines a connection from a router to an external network.
    /// </summary>
    public class ExternalGateway
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalGateway"/> class.
        /// </summary>
        public ExternalGateway()
        {
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
        public bool IsSourceNATEnabled { get; set; }

        /// <summary>
        /// External fixed IP addresses assigned to the router.
        /// </summary>
        [JsonProperty("external_fixed_ips")]
        public IList<IPAddressAssociation> ExternalFixedIPs { get; set; }
    }
}