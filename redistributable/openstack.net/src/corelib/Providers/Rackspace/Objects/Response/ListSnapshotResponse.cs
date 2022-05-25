namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Snapshot Summaries request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/List_Snapshots.html">List Snapshot Summaries (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListSnapshotResponse
    {
        /// <summary>
        /// Gets a collection of information about the snapshots.
        /// </summary>
        [JsonProperty("snapshots")]
        public Snapshot[] Snapshots { get; private set; }
    }
}
