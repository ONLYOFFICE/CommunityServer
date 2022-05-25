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
    /// Reference to a flavor resource.
    /// </summary>
    public class FlavorReference : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The flavor identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="ComputeApi.GetFlavorAsync{T}" />
        /// <exception cref="InvalidOperationException">When the <see cref="FlavorSummary"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task<Flavor> GetFlavorAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            return owner.GetFlavorAsync<Flavor>(Id, cancellationToken);
        }
    }
}