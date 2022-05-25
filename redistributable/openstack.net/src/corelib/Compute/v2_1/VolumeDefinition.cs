using System.Collections.Generic;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines a new volume.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "volume")]
    public class VolumeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeDefinition"/> class.
        /// </summary>
        protected VolumeDefinition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumeDefinition"/> class.
        /// </summary>
        /// <param name="size">The size of the volume, in gigabytes (GB).</param>
        public VolumeDefinition(int size)
        {
            Size = size;
        }

        /// <inheritdoc cref="Volume.Name"/>
        [JsonProperty("display_name")]
        public string Name { get; set; }

        /// <inheritdoc cref="Volume.Description"/>
        [JsonProperty("display_description")]
        public string Description { get; set; }

        /// <inheritdoc cref="Volume.Size"/>
        [JsonProperty("size")]
        public int Size { get; set; }

        /// <inheritdoc cref="Volume.VolumeTypeId"/>
        [JsonProperty("volume_type")]
        public Identifier VolumeTypeId { get; set; }

        /// <inheritdoc cref="Volume.Metadata"/>
        [JsonProperty("metadata")]
        public IDictionary<string, string> Metadata { get; set; }

        /// <inheritdoc cref="Volume.AvailabilityZone"/>
        [JsonProperty("availability_zone")]
        public string AvailabilityZone { get; set; }

        /// <inheritdoc cref="Volume.SourceSnapshotId"/>
        [JsonProperty("snapshot_id")]
        public Identifier SourceSnapshotId { get; set; }
    }
}