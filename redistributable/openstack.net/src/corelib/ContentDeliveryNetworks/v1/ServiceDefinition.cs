using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents the definition of a service resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDefinition"/> class.
        /// </summary>
        /// <param name="name">The service name.</param>
        /// <param name="flavorId">The flavor identifier.</param>
        /// <param name="domain">The service domain.</param>
        /// <param name="origin">The service asset origin.</param>
        public ServiceDefinition(string name, string flavorId, string domain, string origin)
            : this(name, flavorId, new[] {new ServiceDomain(domain)}, new[] {new ServiceOrigin(origin)})
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDefinition"/> class.
        /// </summary>
        /// <param name="name">The service name.</param>
        /// <param name="flavorId">The flavor identifier.</param>
        /// <param name="domains">The service domains.</param>
        /// <param name="origins">The service asset origins.</param>
        public ServiceDefinition(string name, string flavorId, IEnumerable<ServiceDomain> domains, IEnumerable<ServiceOrigin> origins)
        {
            Name = name;
            FlavorId = flavorId;
            Domains = domains.ToNonNullList();
            Origins = origins.ToNonNullList();
            Caches = new List<ServiceCache>();
            Restrictions = new List<ServiceRestriction>();
        }

        /// <summary>
        /// The service name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The identifier of the Content Delivery Network <see cref="Flavor"/> to used by this service.
        /// </summary>
        [JsonProperty("flavor_id")]
        public string FlavorId { get; set; }

        /// <summary>
        /// Domains used by users to access the website.
        /// </summary>
        [JsonProperty("domains")]
        public IList<ServiceDomain> Domains { get; set; }

        /// <summary>
        /// Domains or IP addresses where the canonical assets are stored.
        /// </summary>
        [JsonProperty("origins")]
        public IList<ServiceOrigin> Origins { get; set; }

        /// <summary>
        /// Caching rules for the assets under this service.
        /// </summary>
        [JsonProperty("caching")]
        public IList<ServiceCache> Caches { get; set; }

        /// <summary>
        /// Restrictions defining who can access assets. 
        /// </summary>
        [JsonProperty("restrictions")]
        public IList<ServiceRestriction> Restrictions { get; set; }
    }
}