using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_2
{
    /// <summary />
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "keypair")]
    public class KeyPairDefinition : v2_1.KeyPairDefinition
    {
        /// <inheritdoc />
        public KeyPairDefinition(string name, string publicKey)
            : base(name, publicKey)
        {
        }

        /// <summary />
        [JsonProperty("type")]
        public KeyPairType? Type { get; set; }
    }
}