namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Create Volume and Show Volume requests.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Create_Volume.html">Create Volume (OpenStack Block Storage Service API Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Show_Volume.html">Show Volume (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetCloudBlockStorageVolumeResponse
    {
        /// <summary>
        /// Gets additional information about the volume.
        /// </summary>
        [JsonProperty("volume")]
        public Volume Volume { get; private set; }
    }
}
