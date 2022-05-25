namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Get Server Details, Update Server, and Rebuild Server requests.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Get_Server_Details-d1e2623.html">Get Server Details (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/ServerUpdate.html">Update Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/Rebuild_Server-d1e3538.html">Rebuild Server (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ServerDetailsResponse
    {
        /// <summary>
        /// Gets the detailed information about the server.
        /// </summary>
        [JsonProperty("server")]
        public Server Server { get; private set; }
    }
}
