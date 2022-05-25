namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Create Snapshot and Show Snapshot requests.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Create_Snapshot.html">Create Snapshot (OpenStack Block Storage Service API Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Show_Snapshot.html">Show Snapshot (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetCloudBlockStorageSnapshotResponse
    {
        /// <summary>
        /// Gets information about the snapshot.
        /// </summary>
        [JsonProperty("snapshot")]
        public Snapshot Snapshot { get; private set; }
    }
}
