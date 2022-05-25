namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON body containing details for the Create Snapshot request.
    /// </summary>
    /// <seealso cref="CreateCloudBlockStorageSnapshotRequest"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Create_Snapshot.html">Create Snapshot (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateCloudBlockStorageSnapshotDetails
    {
        /// <summary>
        /// Gets the ID of the volume to snapshot.
        /// </summary>
        [JsonProperty("volume_id")]
        public string VolumeId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to create the snapshot even while the volume is attached to an active server.
        /// </summary>
        /// <value><see langword="true"/> to create the snapshot even if the server is running; otherwise, <see langword="false"/>.</value>
        [JsonProperty("force")]
        public bool Force { get; private set; }

        /// <summary>
        /// Gets the display name of the snapshot.
        /// </summary>
        [JsonProperty("display_name")]
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the display description of the snapshot.
        /// </summary>
        [JsonProperty("display_description")]
        public string DisplayDescription { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCloudBlockStorageSnapshotDetails"/>
        /// class with the specified volume ID, name, description, and value indicating whether or
        /// not to create the snapshot even if the volume is attached to an active server.
        /// </summary>
        /// <param name="volumeId">The ID of the volume to snapshot. The value should be obtained from <see cref="Volume.Id">Volume.Id</see>.</param>
        /// <param name="force"><see langword="true"/> to create the snapshot even if the volume is attached to an active server; otherwise, <see langword="false"/>.</param>
        /// <param name="displayName">Name of the snapshot.</param>
        /// <param name="displayDescription">Description of snapshot.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="volumeId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="volumeId"/> is empty.</exception>
        public CreateCloudBlockStorageSnapshotDetails(string volumeId, bool force, string displayName, string displayDescription)
        {
            if (volumeId == null)
                throw new ArgumentNullException("volumeId");
            if (string.IsNullOrEmpty(volumeId))
                throw new ArgumentException("volumeId cannot be empty");

            VolumeId = volumeId;
            Force = force;
            DisplayName = displayName;
            DisplayDescription = displayDescription;
        }
    }
}
