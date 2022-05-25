namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON body containing details for the Create Volume request.
    /// </summary>
    /// <seealso cref="CreateCloudBlockStorageVolumeRequest"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Create_Volume.html">Create Volume (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateCloudBlockStorageVolumeDetails
    {
        /// <summary>
        /// Gets the size of the volume in GB.
        /// </summary>
        [JsonProperty("size")]
        public int Size { get; private set; }

        /// <summary>
        /// Gets the display description of the volume.
        /// </summary>
        [JsonProperty("display_description")]
        public string DisplayDescription { get; private set; }

        /// <summary>
        /// Gets the display name of the volume.
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the ID of the snapshot to create the volume from, if any.
        /// </summary>
        /// <value>The ID of the snapshot to create the volume from, or <see langword="null"/> if the volume is not created from a snapshot.</value>
        /// <seealso cref="Snapshot.Id"/>
        [JsonProperty("snapshot_id")]
        public string SnapshotId { get; private set; }

        /// <summary>
        /// Gets the ID of the type of volume to create.
        /// </summary>
        /// <seealso cref="net.openstack.Core.Domain.VolumeType.Id"/>
        [JsonProperty("volume_type")]
        public string VolumeType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCloudBlockStorageVolumeDetails"/> class.
        /// </summary>
        /// <param name="size">The size of the volume in GB.</param>
        /// <param name="displayDescription">A description of the volume.</param>
        /// <param name="displayName">The name of the volume.</param>
        /// <param name="snapshotId">The snapshot from which to create a volume. The value should be <see langword="null"/> or obtained from <see cref="Snapshot.Id">Snapshot.Id</see>.</param>
        /// <param name="volumeType">The type of volume to create. If not defined, then the default is used. The value should be <see langword="null"/> or obtained from <see cref="net.openstack.Core.Domain.VolumeType.Id">VolumeType.Id</see>.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="size"/> is less than or equal to zero.</exception>
        public CreateCloudBlockStorageVolumeDetails(int size, string displayDescription, string displayName, string snapshotId, string volumeType)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size");

            Size = size;
            DisplayDescription = displayDescription;
            DisplayName = displayName;
            SnapshotId = snapshotId;
            VolumeType = volumeType;
        }
    }
}
