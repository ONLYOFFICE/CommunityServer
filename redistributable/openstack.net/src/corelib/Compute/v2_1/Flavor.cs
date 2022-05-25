using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Represents a resource configuration for a server.
    /// <para>Each flavor is a unique combination of disk space, memory capacity, vCPUs, and network bandwidth.</para>
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "flavor")]
    public class Flavor : FlavorSummary
    {
        /// <summary>
        /// The disk size.
        /// </summary>
        [JsonProperty("disk")]
        public int DiskSize { get; set; }

        /// <summary>
        /// The amount of RAM.
        /// </summary>
        [JsonProperty("ram")]
        public int MemorySize { get; set; }

        /// <summary>
        /// The amount of swap space.
        /// </summary>
        [JsonProperty("swap")]
        public int? SwapSize { get; set; }

        /// <summary>
        /// The number of virtual CPUs.
        /// </summary>
        [JsonProperty("vcpus")]
        public int VirtualCPUs { get; set; }

        /// <summary>
        /// The rxtx factor, which describes configured bandwidth cap values.
        /// </summary>
        [JsonProperty("rxtx_factor")]
        public double? BandwidthCap { get; set; }

        /// <summary>
        /// The number of ephemeral disks.
        /// </summary>
        [JsonProperty("OS-FLV-EXT-DATA:ephemeral")]
        public int? EphemeralDiskSize { get; set; }
    }
}