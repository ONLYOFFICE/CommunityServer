using System;
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
    /// Summary informationm for a server action.
    /// </summary>
    public class ServerActionSummary : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The action identifier.
        /// </summary>
        [JsonProperty("request_id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The server identifier.
        /// </summary>
        [JsonProperty("instance_uuid")]
        public Identifier ServerId { get; set; }

        /// <summary>
        /// The action name.
        /// </summary>
        [JsonProperty("action")]
        public string Name { get; set; }

        /// <summary>
        /// The action output message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// The identifier of the user who performed the action.
        /// </summary>
        [JsonProperty("user_id")]
        public Identifier UserId { get; set; }

        /// <summary>
        /// The action start time.
        /// </summary>
        [JsonProperty("start_time")]
        public DateTimeOffset Started { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="ComputeApi.GetServerActionAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<ServerAction> GetActionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return compute.GetServerActionAsync<ServerAction>(ServerId, Id, cancellationToken);
        }
    }
}