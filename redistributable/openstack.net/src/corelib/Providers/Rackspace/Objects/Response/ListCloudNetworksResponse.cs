namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Networks request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-network/2.0/content/List_Networks.html">List Networks (OpenStack Networking API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListCloudNetworksResponse
    {
        /// <summary>
        /// Gets a collection of networks.
        /// </summary>
        [JsonProperty("networks")]
        public CloudNetwork[] Networks { get; private set; }
    }
}
