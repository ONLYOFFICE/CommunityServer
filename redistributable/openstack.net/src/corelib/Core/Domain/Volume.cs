namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a volume in a block storage provider.
    /// </summary>
    /// <seealso cref="IBlockStorageProvider"/>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Volume : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the unique identifier for the volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the "display_name" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <remarks>
        /// <note>
        /// This property is a Rackspace-specific extension to the OpenStack Block Storage Service.
        /// </note>
        /// </remarks>
        [JsonProperty("display_name")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the "display_description" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <remarks>
        /// <note>
        /// This property is a Rackspace-specific extension to the OpenStack Block Storage Service.
        /// </note>
        /// </remarks>
        [JsonProperty("display_description")]
        public string DisplayDescription { get; private set; }

        /// <summary>
        /// Gets the "size" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; private set; }

        /// <summary>
        /// Gets the "volume_type" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("volume_type")]
        public string VolumeType { get; private set; }

        /// <summary>
        /// Gets the "snapshot_id" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso cref="Snapshot.Id"/>
        [JsonProperty("snapshot_id")]
        public string SnapshotId { get; private set; }

        /// <summary>
        /// Gets the "attachments" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("attachments")]
        public Dictionary<string, string>[] Attachments { get; private set; }

        /// <summary>
        /// Gets the "status" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("status")]
        public VolumeState Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the "availability_zone" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("availability_zone")]
        public string AvailabilityZone { get; private set; }

        /// <summary>
        /// Gets the "created_at" property of this volume.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; private set; }
    }
}
