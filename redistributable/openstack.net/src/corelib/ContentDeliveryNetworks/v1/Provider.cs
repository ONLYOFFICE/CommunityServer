using System.Collections.Generic;
using net.openstack.Core.Domain;
using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a provider resource of the <see cref="IContentDeliveryNetworkService"/>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class Provider
    {
        /// <summary>
        /// The provider name 
        /// </summary>
        [JsonProperty("provider")]
        public string Name { get; set; }

        /// <summary>
        /// A collection of links related to the provider.
        /// </summary>
        [JsonProperty("links")]
        public IEnumerable<Link> Links { get; set; }
    }
}