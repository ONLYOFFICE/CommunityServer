using Newtonsoft.Json;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines how to boot a server from a volume.
    /// </summary>
    /// <remarks>
    /// If you specify the volume status, you must set it to available. In the OpenStack Block Storage database, the volume attach_status must be detached. 
    /// </remarks>
    public class ServerBlockDeviceMapping
    {
        /// <summary>
        /// Defines the order in which a hypervisor tries devices when it attempts to boot the guest from storage.
        /// </summary>
        [JsonProperty("boot_index", DefaultValueHandling = DefaultValueHandling.Include)]
        public int BootIndex { get; set; }

        /// <summary>
        /// A path to the device for the volume that you want to use to boot the server.
        /// </summary>
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }

        /// <summary>
        /// The identifier of the source block device.
        /// </summary>
        [JsonProperty("uuid")]
        public Identifier SourceId { get; set; }

        /// <summary>
        /// The source type of the volume.
        /// </summary>
        [JsonProperty("source_type")]
        public ServerBlockDeviceType SourceType { get; set; }

        /// <summary>
        /// The source type of the volume. 
        /// </summary>
        [JsonProperty("destination_type")]
        public ServerBlockDeviceType DestinationType { get; set; }

        /// <summary>
        /// The size of the destination volume, in GB.
        /// </summary>
        [JsonProperty("volume_size")]
        public int? DestinationVolumeSize { get; set; }
    
        /// <summary>
        /// Specifies if the volume should be deleted when the server is deleted.
        /// </summary>
        [JsonProperty("delete_on_termination")]
        public bool DeleteWithServer { get; set; }

        /// <summary>
        /// Specifies how/if to format the device prior to attaching, and should be only used with blank local images.
        /// <para>Denotes a swap disk if the value is swap.</para>
        /// </summary>
        [JsonProperty("guest_format")]
        public string GuestFormat { get; set; }
    }
}