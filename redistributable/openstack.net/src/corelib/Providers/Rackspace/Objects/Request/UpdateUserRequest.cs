namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Update User request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUser_v2.0_users__userId__.html">Update User (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateUserRequest
    {
        /// <summary>
        /// Gets the updated user information for the request.
        /// </summary>
        [JsonProperty("user")]
        public User User { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserRequest"/> class
        /// with the specified user.
        /// </summary>
        /// <param name="user">A <see cref="User"/> instance containing the updated details for the user.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="user"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="user"/>.<see cref="net.openstack.Core.Domain.User.Id"/> is <see langword="null"/> or empty.</exception>
        public UpdateUserRequest(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrEmpty(user.Id))
                throw new ArgumentException("user.Id cannot be null or empty");

            User = user;
        }
    }
}
