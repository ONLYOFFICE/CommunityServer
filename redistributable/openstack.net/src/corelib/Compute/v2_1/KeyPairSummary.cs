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
    /// Key pair summary information.
    /// </summary>
    [JsonConverter(typeof(KeyPairConverter))]
    public class KeyPairSummary : IHaveExtraData, IServiceResource
    {
        /// <summary>
        /// The key pair name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The public ssh key.
        /// </summary>
        [JsonProperty("public_key")]
        public string PublicKey { get; set; }

        /// <summary>
        /// A short sequence of bytes used to authenticate, or look up, a longer public key.
        /// </summary>
        [JsonProperty("fingerprint")]
        public string Fingerprint { get; set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        /// <inheritdoc cref="ComputeApi.DeleteKeyPairAsync" />
        public Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            return compute.DeleteKeyPairAsync(Name, cancellationToken);
        }
    }
}