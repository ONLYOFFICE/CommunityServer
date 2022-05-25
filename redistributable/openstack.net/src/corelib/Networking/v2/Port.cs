using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Represents a port resource of the <see cref="NetworkingService"/>.
    /// </summary>
    /// <para>
    /// Virtual (or logical) switch ports on a given network.
    /// </para>
    /// <threadsafety static="true" instance="false"/>
    public class Port : PortCreateDefinition, IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The ID of the port.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }
        
        /// <summary>
        /// The port status.
        /// </summary>
        [JsonProperty("status")]
        public PortStatus Status { get; set; }

        /// <summary>
        /// The administrative state of the port.
        /// </summary>
        [JsonProperty("admin_state_up")]
        public bool IsUp { get; set; }

        /// <summary>
        /// The port security enablement status.
        /// </summary>
        [JsonProperty("port_security_enabled")]
        public bool IsPortSecurityEnabled { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }
    }
}
