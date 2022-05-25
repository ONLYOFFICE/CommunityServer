using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of keypair resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class KeyPairCollection<T> : ResourceCollection<T>
        where T : IServiceResource
    {
        /// <summary/>
        [JsonProperty("keypairs")]
        protected IList<T> KeyPairs => Items;
    }

    /// <summary>
    /// Represents a collection of key pair summary resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class KeyPairSummaryCollection : KeyPairCollection<KeyPairSummary>
    { }
}
