namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Volume Types request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/Volume_List_Types.html">List Volume Types (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListVolumeTypeResponse
    {
        /// <summary>
        /// Gets a collection of information about the volume types.
        /// </summary>
        [JsonProperty("volume_types")]
        public VolumeType[] VolumeTypes { get; private set; }
    }
}
