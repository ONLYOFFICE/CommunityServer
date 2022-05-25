namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the Authenticate request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AuthRequest
    {
        /// <summary>
        /// Gets additional information about the credentials to authenticate.
        /// </summary>
        [JsonProperty("auth")]
        public AuthDetails Credentials { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthRequest"/> class with the
        /// given identity.
        /// </summary>
        /// <param name="identity">The identity of the user to authenticate.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="identity"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">If given <paramref name="identity"/> type is not supported.</exception>
        public AuthRequest(CloudIdentity identity)
        {
            if (identity == null)
                throw new ArgumentNullException("identity");

            var credentials = new AuthDetails();
            if (string.IsNullOrEmpty(identity.Password))
                credentials.APIKeyCredentials = new Credentials(identity.Username, null, identity.APIKey);
            else
                credentials.PasswordCredentials = new Credentials(identity.Username, identity.Password, null);

            var raxIdentity = identity as RackspaceCloudIdentity;
            if (raxIdentity != null)
                credentials.Domain = raxIdentity.Domain;

            Credentials = credentials;
        }
    }
}
