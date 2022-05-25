using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Defines a new router instance.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "router")]
    public class RouterCreateDefinition
    {
        /// <inheritdoc cref="Router.Name" />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc cref="Router.ExternalGateway" />
        [JsonProperty("external_gateway_info")]
        public ExternalGatewayDefinition ExternalGateway { get; set; }

        /// <inheritdoc cref="Router.IsUp" />
        [JsonProperty("admin_state_up")]
        public bool? IsUp { get; set; }
    }
}