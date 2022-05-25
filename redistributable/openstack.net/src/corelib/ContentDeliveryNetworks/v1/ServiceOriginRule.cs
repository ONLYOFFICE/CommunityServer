using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a service origin rule resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceOriginRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceOriginRule"/> class.
        /// </summary>
        /// <param name="name">The rule name.</param>
        /// <param name="requestUrl">The request URL this rule should match for this origin to be used.</param>
        public ServiceOriginRule(string name, string requestUrl)
        {
            Name = name;
            RequestUrl = requestUrl;
        }

        /// <summary>
        /// The rule name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// The request URL this rule should match for this origin to be used. (regex is supported).
        /// </summary>
        [JsonProperty("request_url")]
        public string RequestUrl { get; private set; }
    }
}