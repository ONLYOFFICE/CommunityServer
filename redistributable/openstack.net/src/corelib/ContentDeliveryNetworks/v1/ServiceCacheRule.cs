using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a caching rule resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceCacheRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCacheRule"/> class.
        /// </summary>
        /// <param name="name">The rule name.</param>
        /// <param name="requestUrl">The request rule.</param>
        public ServiceCacheRule(string name, string requestUrl)
        {
            Name = name;
            RequestUrl = requestUrl;
        }

        /// <summary>
        /// The rule name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The request url this rule should match for this TTL to be used (regex is supported).
        /// </summary>
        [JsonProperty("request_url")]
        public string RequestUrl { get; set; }
    }
}