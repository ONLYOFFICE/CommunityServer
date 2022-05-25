namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Providers.Rackspace.Objects.Request;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Update User Credentials request
    /// when used with password credentials.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_updateUserCredential_v2.0_users__userId__OS-KSADM_credentials__credential-type__.html">Update User Credentials (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class PasswordCredentialResponse
    {
        /// <summary>
        /// Gets the password credentials used for the Update User Credentials request.
        /// </summary>
        [JsonProperty("passwordCredentials")]
        public PasswordCredential PasswordCredential { get; private set; }
    }
}
