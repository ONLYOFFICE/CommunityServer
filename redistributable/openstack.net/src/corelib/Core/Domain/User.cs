namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a user account.
    /// </summary>
    /// <seealso cref="IIdentityProvider.ListUsers"/>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class User : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="DomainId"/> property.
        /// </summary>
        [JsonProperty("RAX-AUTH:domainId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ProjectId _domainId;
#pragma warning restore 649

        /// <summary>
        /// Gets or sets the default region of the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <remarks>
        /// Changes to this property are not automatically saved on the server. To apply the
        /// changes, call <see cref="IIdentityProvider.UpdateUser"/> after setting this property.
        ///
        /// <note>
        /// This property is a Rackspace-specific extension to the OpenStack Identity Service.
        /// </note>
        /// </remarks>
        [JsonProperty("RAX-AUTH:defaultRegion")]
        public string DefaultRegion { get; set; }

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

        /// <summary>
        /// Gets the unique identifier for the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets the "username" property of the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <remarks>
        /// Changes to this property are not automatically saved on the server. To apply the
        /// changes, call <see cref="IIdentityProvider.UpdateUser"/> after setting this property.
        /// </remarks>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the "email" property of the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <remarks>
        /// Changes to this property are not automatically saved on the server. To apply the
        /// changes, call <see cref="IIdentityProvider.UpdateUser"/> after setting this property.
        /// </remarks>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the "enabled" property of the user.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <remarks>
        /// Changes to this property are not automatically saved on the server. To apply the
        /// changes, call <see cref="IIdentityProvider.UpdateUser"/> after setting this property.
        /// </remarks>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
    }
}
