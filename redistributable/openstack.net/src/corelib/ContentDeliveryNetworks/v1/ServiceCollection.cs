using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a collection of service resources of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceCollection : Page<ServiceCollection, Service, PageLink>
    {
        /// <summary>
        /// The requested services.
        /// </summary>
        [JsonProperty("services")]
        protected IList<Service> Services => Items;

        /// <summary>
        /// The paging navigation links.
        /// </summary>
        [JsonProperty("links")]
        protected IList<PageLink> ServiceLinks => Links;
    }
}