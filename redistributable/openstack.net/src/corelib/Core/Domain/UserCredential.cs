namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a set of credentials for a user.
    /// </summary>
    /// <seealso cref="IIdentityProvider.ListUserCredentials"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listCredentials_v2.0_users__userId__OS-KSADM_credentials_.html">List Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserCredential : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the "name" property for the credentials.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the "username" property for the credentials.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; private set; }

        /// <summary>
        /// Gets the "apiKey" property for the credentials.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("apiKey")]
        public string APIKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserCredential"/> class
        /// with the specified name, username, and API key.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="username">The username.</param>
        /// <param name="apiKey">The API key.</param>
        public UserCredential(string name, string username, string apiKey)
        {
            Name = name;
            Username = username;
            APIKey = apiKey;
        }
    }
}
