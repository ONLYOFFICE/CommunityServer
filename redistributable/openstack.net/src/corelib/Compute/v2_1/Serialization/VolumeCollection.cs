using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of volume resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class VolumeCollection<T> : ResourceCollection<T>
        where T : IServiceResource
    {
        /// <summary />
        [JsonProperty("volumes")]
        protected IList<T> Volumes => Items;
    }

    /// <summary>
    /// Represents a collection of volume resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class VolumeCollection : VolumeCollection<Volume>
    { }
}
