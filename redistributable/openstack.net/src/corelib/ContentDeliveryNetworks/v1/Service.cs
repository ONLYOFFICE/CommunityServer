using System.Collections.Generic;
using net.openstack.Core.Domain;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a service resource of the <see cref="IContentDeliveryNetworkService"/>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class Service : IServiceResource
    {
        /// <summary>
        /// The service identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The service name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The status of the service.
        /// </summary>
        [JsonProperty("status")]
        public ServiceStatus Status { get; set; }

        /// <summary>
        /// The identifier of the Content Delivery Network <see cref="Flavor"/> used by this service.
        /// </summary>
        [JsonProperty("flavor_id")]
        public string FlavorId { get; set; }

        /// <summary>
        /// Domains used by users to access the website.
        /// </summary>
        [JsonProperty("domains")]
        public IEnumerable<ServiceDomain> Domains { get; set; }

        /// <summary>
        /// Domains or IP addresses where the canonical assets are stored.
        /// </summary>
        [JsonProperty("origins")]
        public IEnumerable<ServiceOrigin> Origins { get; set; }

        /// <summary>
        /// Caching rules for the assets under this service.
        /// </summary>
        [JsonProperty("caching")]
        public IEnumerable<ServiceCache> Caches { get; set; }

        /// <summary>
        /// Restrictions defining who can access assets. 
        /// </summary>
        [JsonProperty("restrictions")]
        public IEnumerable<ServiceRestriction> Restrictions { get; set; }

        /// <summary>
        /// Any errors that occurred during the previous service operation.
        /// </summary>
        [JsonProperty("errors")]
        public IEnumerable<ServiceError> Errors { get; set; }

        /// <summary>
        /// Links related to the service.
        /// </summary>
        [JsonProperty("links")]
        public IEnumerable<Link> Links { get; set; }

        object IServiceResource.Owner { get; set; }
    }
}
