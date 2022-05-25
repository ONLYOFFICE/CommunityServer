using Newtonsoft.Json;

namespace OpenStack.Networking.v2.Operator
{
    /// <summary>
    /// Extended definition of a network resource, with cloud operator functionality exposed.
    /// </summary>
    public class NetworkDefinition : v2.NetworkDefinition
    {
        /// <inheritdoc cref="Network.IsExternal" />
        [JsonProperty("router:external")]
        public bool? IsExternal { get; set; }
    }
}
