namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Impersonate User request.
    /// </summary>
    /// <remarks>
    /// The Impersonate User API is a Rackspace-specific extension to the OpenStack
    /// Identity Service, and is documented in the Rackspace <strong>Cloud Identity
    /// Admin Developer Guide - API v2.0</strong>.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UserImpersonationResponse
    {
        /// <summary>
        /// Gets the details for the response.
        /// </summary>
        [JsonProperty("access")]
        public UserImpersonationData UserAccess { get; private set; }

        /// <summary>
        /// This models the JSON body containing details for the Impersonate User response.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        [JsonObject(MemberSerialization.OptIn)]
        internal class UserImpersonationData
        {
            /// <summary>
            /// Gets the <see cref="IdentityToken"/> which allows providers to make
            /// impersonated calls to API methods.
            /// </summary>
            [JsonProperty("token")]
            public IdentityToken Token
            {
                get;
                private set;
            }
        }
    }
}
