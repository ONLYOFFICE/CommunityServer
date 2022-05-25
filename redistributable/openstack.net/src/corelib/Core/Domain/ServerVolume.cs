namespace net.openstack.Core.Domain
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON description of a volume attachment.
    /// </summary>
    /// <remarks>
    /// <note>Volume attachments are a Rackspace-specific extension to the OpenStack Compute Service.</note>
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/List_Volume_Attachments.html">List Volume Attachments (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerVolume : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the "device" property associated with the volume attachment.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; private set; }

        /// <summary>
        /// Gets the "serverId" property associated with the volume attachment.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso cref="ServerBase.Id"/>
        [JsonProperty("serverId")]
        public string ServerId { get; private set; }

        /// <summary>
        /// Gets the unique identifier for the volume attachment.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the "volumeId" property associated with the volume attachment.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso cref="Volume.Id"/>
        [JsonProperty("volumeId")]
        public string VolumeId { get; private set; }
    }
}
