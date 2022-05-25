using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Represents the definition of a network resource of the <see cref="NetworkingService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "network")]
    public class NetworkDefinition : IHaveExtraData
    {
        /// <inheritdoc cref="Network.Name" />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc cref="Network.IsUp" />
        [JsonProperty("admin_state_up")]
        public bool? IsUp { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();
    }
}