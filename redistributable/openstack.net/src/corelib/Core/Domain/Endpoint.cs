namespace net.openstack.Core.Domain
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an endpoint for a service provided in the <see cref="ServiceCatalog"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Endpoint : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the public URL of the service.
        /// </summary>
        [JsonProperty("publicURL")]
        public string PublicURL { get; private set; }

        /// <summary>
        /// Gets the region where this service endpoint is located. If this is <see langword="null"/>
        /// or empty, the region is not specified.
        /// </summary>
        [JsonProperty("region")]
        public string Region { get; private set; }

        /// <summary>
        /// Gets the tenant (or account) ID which this endpoint operates on.
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the "versionId" property associated with the endpoint.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("versionId")]
        public string VersionId { get; private set; }

        /// <summary>
        /// Gets the "versionInfo" property associated with the endpoint.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("versionInfo")]
        public string VersionInfo { get; private set; }

        /// <summary>
        /// Gets the "versionList" property associated with the endpoint.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("versionList")]
        public string VersionList { get; private set; }

        /// <summary>
        /// Gets the internal URL of the service. If this is <see langword="null"/> or empty,
        /// the service should be accessed using the <see cref="PublicURL"/>.
        /// </summary>
        [JsonProperty("internalURL")]
        public string InternalURL { get; private set; }
    }
}
