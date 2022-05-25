using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines how to attach a volume to a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "volumeAttachment")]
    public class ServerVolumeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerVolumeDefinition"/> class.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        public ServerVolumeDefinition(Identifier volumeId)
        {
            VolumeId = volumeId;
        }

        /// <inheritdoc cref="ServerVolume.DeviceName" />
        [JsonProperty("device")]
        public string DeviceName { get; set; }

        /// <inheritdoc cref="ServerVolume.VolumeId" />
        [JsonProperty("volumeId")]
        public Identifier VolumeId { get; set; }
    }
}
