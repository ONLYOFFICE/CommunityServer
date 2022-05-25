namespace net.openstack.Core.Domain
{
    using System;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the basic JSON description of a snapshot.
    /// </summary>
    /// <seealso cref="IBlockStorageProvider.ListSnapshots"/>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Snapshot : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the unique identifier for the snapshot.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the snapshot.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the description of the snapshot.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("display_description")]
        public string DisplayDescription { get; private set; }

        /// <summary>
        /// Gets the ID of the volume this snapshot was taken from.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso cref="Volume.Id"/>
        [JsonProperty("volume_id")]
        public string VolumeId { get; private set; }

        /// <summary>
        /// Gets the status of the snapshot.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("status")]
        public SnapshotState Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the "size" property of the snapshot.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("size")]
        public string Size { get; private set; }

        /// <summary>
        /// Gets the "created_at" property of the snapshot.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; private set; }
    }
}
