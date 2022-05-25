using Newtonsoft.Json;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// The generated key pair.
    /// <para>Includes the generated private key, which is sensitive data.</para>
    /// </summary>
    public class KeyPairResponse : KeyPairSummary
    {
        /// <summary>
        /// The private ssh key.
        /// </summary>
        [JsonProperty("private_key")]
        public string PrivateKey { get; set; }
    }
}