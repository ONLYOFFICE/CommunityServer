using System.Collections.Generic;

using net.openstack.Core.Domain;

using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a flavor resource of the <see cref="IContentDeliveryNetworkService"/>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class Flavor
    {
        /// <summary>
        /// The unique identifier for the flavor.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// Collection of links related to the current flavor.
        /// </summary>
        [JsonProperty("links")]
        public IEnumerable<Link> Links { get; set; }

        /// <summary>
        /// Collection of available providers for the current flavor.
        /// </summary>
        [JsonProperty("providers")]
        public IEnumerable<Provider> Providers { get; set; }
    }
}
