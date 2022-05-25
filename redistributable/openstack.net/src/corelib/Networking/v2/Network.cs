using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Represents a network resource of the <see cref="NetworkingService"/>
    /// <para>
    /// Isolated virtual Layer-2 domains; a network can also be regarded as a virtual (or logical) switch.
    /// </para>
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "network")]
    public class Network : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The network identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The network name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The administrative state of the network.
        /// </summary>
        [JsonProperty("admin_state_up")]
        public bool IsUp { get; set; }

        /// <summary>
        /// Indicates whether this network is shared across all tenants.
        /// </summary>
        [JsonProperty("shared")]
        public bool IsShared { get; set; }

        /// <summary>
        /// Indicates whether this network is externally accessible.
        /// </summary>
        [JsonProperty("router:external")]
        public bool IsExternal { get; set; }

        /// <summary>
        /// The network status.
        /// </summary>
        [JsonProperty("status")]
        public NetworkStatus Status { get; set; }

        /// <summary>
        /// The associated subnet identifiers.
        /// </summary>
        [JsonProperty("subnets")]
        public IList<Identifier> Subnets { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }
    }
}