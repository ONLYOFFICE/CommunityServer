using System;
using Newtonsoft.Json;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Key kair resource.
    /// <para>Does not include the private key.</para>
    /// </summary>
    public class KeyPair : KeyPairSummary
    {
        /// <summary>
        /// The key pair identifier.
        /// </summary>
        [JsonProperty("id")]
        public Identifier Id { get; set; }

        /// <summary>
        /// The date and time when the resource was created.
        /// </summary>
        [JsonProperty("created_at")]
        public DateTimeOffset Created { get; set; }
    }
}