using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Represents a collection of volume snapshot resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class VolumeSnapshotCollection<T> : ResourceCollection<T>
    {
        /// <summary />
        [JsonProperty("snapshots")]
        protected IList<T> VolumeSnapshots => Items;
    }

    /// <summary>
    /// Represents a collection of volume snapshot resources of the <see cref="ComputeService"/>.
    /// </summary>
    /// <exclude />
    public class VolumeSnapshotCollection : VolumeSnapshotCollection<VolumeSnapshot>
    { }
}
