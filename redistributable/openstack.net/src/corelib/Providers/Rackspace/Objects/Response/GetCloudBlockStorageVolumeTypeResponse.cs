namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Show Volume Type request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Volume_Show_Type.html">Show Volume Type (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetCloudBlockStorageVolumeTypeResponse
    {
        /// <summary>
        /// Gets additional information about the volume type.
        /// </summary>
        [JsonProperty("volume_type")]
        public VolumeType VolumeType { get; private set; }
    }
}
