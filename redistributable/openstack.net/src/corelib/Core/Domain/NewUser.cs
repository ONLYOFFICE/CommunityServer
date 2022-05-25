namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the JSON result of an Add User operation.
    /// </summary>
    /// <seealso cref="IIdentityProvider.AddUser"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_addUser_v2.0_users_.html">Add User (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <seealso href="http://docs.rackspace.com/auth/api/v2.0/auth-client-devguide/content/POST_addUser_v2.0_users_.html">Add User (Rackspace Cloud Identity Client Developer Guide - API v2.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewUser : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the password for the new user.
        /// </summary>
        /// <value>The generated password for the new user, or <see langword="null"/> if the Add User request included a password.</value>
        [JsonProperty("OS-KSADM:password")]
        public string Password { get; internal set; }

        /// <summary>
        /// Gets the ID for the new user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the username of the new user.
        /// <note type="warning">
        /// The value of this property in the underlying JSON representation differs between
        /// Rackspace and the OpenStack OS-KSADM extension. This property models the
        /// Rackspace-specific representation of the resource.
        /// </note>
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; private set; }

        /// <summary>
        /// Gets the email address of the new user.
        /// </summary>
        /// <value>
        /// The email address of the user.
        /// <para>-or-</para>
        /// <para><see langword="null"/> if the response from the server did not include the underlying property.</para>
        /// </value>
        [JsonProperty("email")]
        public string Email { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the user is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the user is enabled; otherwise, <see langword="false"/>.
        /// </value>
        [JsonProperty("enabled")]
        public bool Enabled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewUser"/> class with the specified
        /// username, email address, password, and value indicating whether or not the user
        /// is initially enabled.
        /// </summary>
        /// <param name="username">The username of the new user (see <see cref="Username"/>).</param>
        /// <param name="email">The email address of the new user (see <see cref="Email"/>).</param>
        /// <param name="password">The password for the new user (see <see cref="Password"/>).</param>
        /// <param name="enabled"><see langword="true"/> if the user is initially enabled; otherwise, <see langword="false"/> (see <see cref="Enabled"/>).</param>
        public NewUser(string username, string email, string password = null, bool enabled = true)
        {
            Username = username;
            Email = email;
            Password = password;
            Enabled = enabled;
        }
    }
}
