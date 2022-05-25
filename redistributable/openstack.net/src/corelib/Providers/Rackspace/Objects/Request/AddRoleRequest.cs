namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Add Role request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_addRole_v2.0_OS-KSADM_roles_Role_Operations_OS-KSADM.html">Add Role (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AddRoleRequest
    {
        /// <summary>
        /// Gets additional information about the role to add.
        /// </summary>
        [JsonProperty("role")]
        public Role Role { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddRoleRequest"/> class for the
        /// specified <paramref name="role"/>.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="role"/> is <see langword="null"/>.</exception>
        public AddRoleRequest(Role role)
        {
            if (role == null)
                throw new ArgumentNullException("role");

            Role = role;
        }
    }
}
