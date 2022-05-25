using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a service domain resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceDomain
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDomain"/> class.
        /// </summary>
        /// <param name="domain">The domain name.</param>
        public ServiceDomain(string domain)
        {
            Domain = domain;
        }

        /// <summary>
        /// The domain used to access the assets on their website, which will have a CNAME entry pointed back to the CDN provider.
        /// </summary>
        [JsonProperty("domain")]
        public string Domain { get; set; }

        /// <summary>
        /// The protocol used to access the assets on this domain.
        /// </summary>
        [JsonProperty("protocol")]
        public ServiceProtocol? Protocol { get; set; }
    }
}