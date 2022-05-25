namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Create Snapshot request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Create_Snapshot.html">Create Snapshot (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateCloudBlockStorageSnapshotRequest
    {
        /// <summary>
        /// Gets additional details about the Create Snapshot request.
        /// </summary>
        [JsonProperty("snapshot")]
        public CreateCloudBlockStorageSnapshotDetails CreateCloudBlockStorageSnapshotDetails { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateCloudBlockStorageSnapshotRequest"/>
        /// class with the specified details.
        /// </summary>
        /// <param name="details">The details of the request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="details"/> is <see langword="null"/>.</exception>
        public CreateCloudBlockStorageSnapshotRequest(CreateCloudBlockStorageSnapshotDetails details)
        {
            if (details == null)
                throw new ArgumentNullException("details");

            CreateCloudBlockStorageSnapshotDetails = details;
        }
    }
}
