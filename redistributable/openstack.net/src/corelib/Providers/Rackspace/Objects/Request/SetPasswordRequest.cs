namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Update User Credentials request
    /// when used with password credentials.
    /// </summary>
    /// <seealso cref="PasswordCredential"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class SetPasswordRequest
    {
        /// <summary>
        /// Gets the password credentials to use for the Update User Credentials request.
        /// </summary>
        [JsonProperty("passwordCredentials")]
        public PasswordCredential PasswordCredential { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetPasswordRequest"/> class
        /// with the specified username and password.
        /// </summary>
        /// <param name="username">The new username.</param>
        /// <param name="password">The new password.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="username"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="username"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is empty.</para>
        /// </exception>
        public SetPasswordRequest(string username, string password)
        {
            if (username == null)
                throw new ArgumentNullException("username");
            if (password == null)
                throw new ArgumentNullException("password");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("username cannot be empty");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password cannot be empty");

            PasswordCredential = new PasswordCredential(username, password);
        }
    }
}
