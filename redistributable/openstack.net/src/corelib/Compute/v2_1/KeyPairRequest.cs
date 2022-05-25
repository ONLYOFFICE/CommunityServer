using Newtonsoft.Json;
using OpenStack.Compute.v2_1.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// A request to generate a key pair.
    /// </summary>
    [JsonConverter(typeof(KeyPairConverter))]
    public class KeyPairRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPairRequest"/> class.
        /// </summary>
        /// <param name="name">The key pair name.</param>
        public KeyPairRequest(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The key pair name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}