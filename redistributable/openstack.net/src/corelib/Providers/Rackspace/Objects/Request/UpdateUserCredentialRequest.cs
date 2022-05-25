namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Update User Credentials request
    /// when used with API Key credentials.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateUserCredentialRequest
    {
        /// <summary>
        /// Gets the API Key credentials to use for the Update User Credentials request.
        /// </summary>
        [JsonProperty("RAX-KSKEY:apiKeyCredentials")]
        public UserCredential UserCredential { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateUserCredentialRequest"/>
        /// class with the specified API Key credentials.
        /// </summary>
        /// <param name="username">The new username.</param>
        /// <param name="apiKey">The new API key.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="username"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="apiKey"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="username"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="apiKey"/> is empty.</para>
        /// </exception>
        public UpdateUserCredentialRequest(string username, string apiKey)
        {
            if (username == null)
                throw new ArgumentNullException("username");
            if (apiKey == null)
                throw new ArgumentNullException("apiKey");
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("username cannot be empty");
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("apiKey cannot be empty");

            UserCredential = new UserCredential(null, username, apiKey);
        }
    }
}
