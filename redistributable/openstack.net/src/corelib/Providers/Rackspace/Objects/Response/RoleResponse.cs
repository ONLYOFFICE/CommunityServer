namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Add Role and Get Role by Name requests.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_addRole_v2.0_OS-KSADM_roles_Role_Operations_OS-KSADM.html">Add Role (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_getRoleByName_v2.0_OS-KSADM_roles_Role_Operations_OS-KSADM.html">Get Role by Name (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class RoleResponse
    {
        /// <summary>
        /// Gets information about the role.
        /// </summary>
        [JsonProperty("role")]
        public Role Role { get; private set; }
    }
}
