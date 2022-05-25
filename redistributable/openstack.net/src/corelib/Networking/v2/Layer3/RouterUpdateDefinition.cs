using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Defines a set of fields to update on a router.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "router")]
    public class RouterUpdateDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Router"/> class.
        /// </summary>
        public RouterUpdateDefinition()
        {
            Routes = new List<HostRoute>();
        }

        /// <inheritdoc cref="Router.Name" />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc cref="Router.ExternalGateway" />
        [JsonProperty("external_gateway_info")]
        public ExternalGatewayDefinition ExternalGateway { get; set; }

        /// <inheritdoc cref="Router.IsUp" />
        [JsonProperty("admin_state_up")]
        public bool? IsUp { get; set; }

        /// <inheritdoc cref="Router.Routes" />
        [JsonProperty("routes")]
        public IList<HostRoute> Routes { get; set; }
    }
}