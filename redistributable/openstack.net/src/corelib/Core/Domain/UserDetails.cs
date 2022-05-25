namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains additional information about an authenticated user.
    /// </summary>
    /// <seealso cref="UserAccess.User"/>
    /// <seealso cref="IIdentityProvider.Authenticate"/>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserDetails : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="DomainId"/> property.
        /// </summary>
        [JsonProperty("RAX-AUTH:domainId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ProjectId _domainId;
#pragma warning restore 649

        /// <summary>
        /// Gets the unique identifier for the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the "name" property for the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the "roles" property for the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("roles")]
        public Role[] Roles { get; private set; }

        /// <summary>
        /// Gets the default region for the user.
        /// </summary>
        /// <remarks>
        /// Users can be assigned a default region so that, when there is a choice between
        /// multiple endpoints associated with a service in the user's catalog, the endpoint
        /// for the user's default region will be selected if it is available. The default
        /// region is only used when a region is not explicitly specified in the API call.
        ///
        /// <note>
        /// This property is a Rackspace-specific extension to the OpenStack Identity Service.
        /// </note>
        /// </remarks>
        /// <seealso href="http://docs.rackspace.com/auth/api/v2.0/auth-client-devguide/content/Sample_Request_Response-d1e64.html">Sample Authentication Request and Response (Rackspace Cloud Identity Client Developer Guide - API v2.0)</seealso>
        [JsonProperty("RAX-AUTH:defaultRegion")]
        public string DefaultRegion { get; private set; }

        /// <summary>
        /// Gets the domain ID of the user.
        /// </summary>
        /// <remarks>
        /// <note>
        /// This property is a Rackspace-specific extension to the OpenStack Identity Service.
        /// </note>
        /// </remarks>
        /// <preliminary/>
        public ProjectId DomainId
        {
            get
            {
                return _domainId;
            }
        }
    }
}
