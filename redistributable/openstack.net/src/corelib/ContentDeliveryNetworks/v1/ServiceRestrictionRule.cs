using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a service restriction rule resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceRestrictionRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRestrictionRule"/> class.
        /// </summary>
        /// <param name="name">The restriction name.</param>
        /// <param name="referrer">The http host from which requests must originate.</param>
        public ServiceRestrictionRule(string name, string referrer)
        {
            Name = name;
            Referrer = referrer;
        }

        /// <summary>
        /// The restriction name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The http host from which requests must originate.
        /// </summary>
        [JsonProperty("referrer")]
        public string Referrer { get; set; }
    }
}