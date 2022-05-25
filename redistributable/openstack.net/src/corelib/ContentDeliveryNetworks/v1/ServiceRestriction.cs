using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Represents a service restriction resource of the <see cref="IContentDeliveryNetworkService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class ServiceRestriction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRestriction"/> class.
        /// </summary>
        /// <param name="name">The restriction name.</param>
        /// <param name="rules">The rules that define the restrictions to impose.</param>
        public ServiceRestriction(string name, IEnumerable<ServiceRestrictionRule> rules)
        {
            Name = name;
            Rules = rules.ToNonNullList();
        }

        /// <summary>
        /// The restriction name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Rules that define the restrictions to impose.
        /// </summary>
        [JsonProperty("rules")]
        public IList<ServiceRestrictionRule> Rules { get; set; }
    }
}