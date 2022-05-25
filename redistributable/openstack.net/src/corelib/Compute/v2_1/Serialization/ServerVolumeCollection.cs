using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of server volume resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ServerVolumeCollection<T> : ResourceCollection<T>
        where T : IServiceResource
    {
        /// <summary />
        [JsonProperty("volumeAttachments")]
        protected IList<T> Volumes => Items;
    }

    /// <summary>
    /// Represents a collection of references to server volume resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class ServerVolumeCollection : ServerVolumeCollection<ServerVolume>
    { }
}
