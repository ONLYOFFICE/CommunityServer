using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Represents a volume attached to a server.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "volumeAttachment")]
    public class ServerVolume : ServerVolumeReference
    {
        private Identifier _serverId;

        /// <summary>
        /// A path to the device for the volume attached to the server.
        /// </summary>
        [JsonProperty("device")]
        public string DeviceName { get; set; }

        /// <summary>
        /// The server identifier.
        /// </summary>
        [JsonProperty("serverId")]
        public Identifier ServerId
        {
            get { return _serverId; }
            set
            {
                _serverId = value;
                ((IChildResource)this).SetParent(_serverId);
            }
        }

        /// <summary>
        /// The volume identifier.
        /// </summary>
        [JsonProperty("volumeId")]
        public Identifier VolumeId { get; set; }
    }
}
