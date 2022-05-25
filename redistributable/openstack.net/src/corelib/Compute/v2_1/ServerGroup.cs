using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// A set of server instances used to apply common policies.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "server_group")]
    public class ServerGroup : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerGroup"/> class.
        /// </summary>
        public ServerGroup()
        {
            Policies = new List<string>();
        }

        /// <summary>
        /// The server group identitifer.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The server group name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// A list of one or more policy names to associate with the server group.
        /// </summary>
        [JsonProperty("policies")]
        public IList<string> Policies { get; set; }

        /// <summary>
        /// The servers included in the group.
        /// </summary>
        [JsonProperty("members")]
        public IList<Identifier> Members { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="ComputeApi.DeleteServerGroupAsync" />
        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.DeleteServerGroupAsync(Id, cancellationToken);
        }
    }
}