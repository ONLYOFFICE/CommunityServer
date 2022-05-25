namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Add User request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_addUser_v2.0_users_.html">Add User (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AddUserRequest
    {
        /// <summary>
        /// Gets additional information about the user to add.
        /// </summary>
        [JsonProperty("user")]
        public NewUser User { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddUserRequest"/> class for the
        /// specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="user"/> is <see langword="null"/>.</exception>
        public AddUserRequest(NewUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            User = user;
        }
    }
}
