namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Volume Summaries request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-block-storage/2.0/content/List_Summary_Volumes.html">List Volume Summaries (OpenStack Block Storage Service API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListVolumeResponse
    {
        /// <summary>
        /// Gets a collection of information about the volumes.
        /// </summary>
        [JsonProperty("volumes")]
        public Volume[] Volumes { get; private set; }
    }
}
