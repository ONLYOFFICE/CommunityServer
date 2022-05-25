namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Servers and List Servers with Details requests.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Servers-d1e2078.html">List Servers (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListServersResponse
    {
        /// <summary>
        /// Gets a collection of information about the servers.
        /// </summary>
        [JsonProperty("servers")]
        public Server[] Servers { get; private set; }

        /// <summary>
        /// Gets a collection of links related to the collection of servers.
        /// </summary>
        [JsonProperty("servers_links")]
        public Link[] Links { get; private set; }
    }
}
