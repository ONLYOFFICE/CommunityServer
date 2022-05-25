using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines a new server group.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "server_group")]
    public class ServerGroupDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerGroupDefinition"/> class.
        /// </summary>
        public ServerGroupDefinition(string name, params string[] policies)
        {
            Name = name;
            Policies = new List<string>(policies);
        }

        /// <inheritdoc cref="ServerGroup.Name" />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc cref="ServerGroup.Policies" />
        [JsonProperty("policies")]
        public IList<string> Policies { get; set; }
    }
}