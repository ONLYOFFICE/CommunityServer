namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Users request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listUsers_v2.0_users_.html">List Users (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UsersResponse
    {
        /// <summary>
        /// Gets a collection of information about the users.
        /// </summary>
        [JsonProperty("users")]
        public User[] Users { get; private set; }
    }
}
