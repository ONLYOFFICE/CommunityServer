namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Get User Information by Name, Get User Information by ID, and Update User requests.
    /// </summary>
    /// <remarks>
    /// In certain situations with certain vendors, the List Users request is known to result
    /// in a response that resembles this model. When such a situation is detected, this
    /// response model is also used for the List Users response.
    /// </remarks>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_getUserByName_v2.0_users__User_Operations.html">Get User Information by Name (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_getUserById_v2.0_users__user_id__User_Operations.html">Get User Information by ID (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUser_v2.0_users__userId__.html">Update User (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listUsers_v2.0_users_.html">List Users (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UserResponse
    {
        /// <summary>
        /// Gets information about the user.
        /// </summary>
        [JsonProperty("user")]
        public User User { get; private set; }
    }
}
