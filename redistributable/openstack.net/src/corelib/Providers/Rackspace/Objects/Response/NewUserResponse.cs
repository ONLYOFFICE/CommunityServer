namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Add User request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_addUser_v2.0_users_.html">Add User (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class NewUserResponse
    {
        /// <summary>
        /// Gets the information about the newly created user, including the generated
        /// <see cref="net.openstack.Core.Domain.NewUser.Password"/> if no password was specified in the request.
        /// </summary>
        [JsonProperty("user")]
        public NewUser NewUser { get; private set; }
    }
}
