namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Create Network and Show Network requests.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-network/2.0/content/Create_Network.html">Create Network (OpenStack Networking API v2.0 Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-network/2.0/content/List_Networks_Detail.html">Show Network (OpenStack Networking API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CloudNetworkResponse
    {
        /// <summary>
        /// Gets additional information about the network.
        /// </summary>
        [JsonProperty("network")]
        public CloudNetwork Network { get; private set; }
    }
}
