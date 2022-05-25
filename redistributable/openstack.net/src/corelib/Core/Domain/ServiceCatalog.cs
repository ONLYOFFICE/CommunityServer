namespace net.openstack.Core.Domain
{
    using System.Diagnostics;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a single service provided to an authenticated user. Each service
    /// has one or more <see cref="Endpoints"/> providing access information for the
    /// service.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Name,nq} ({Type,nq})")]
    public class ServiceCatalog : ExtensibleJsonObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCatalog"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by the JSON deserializer.
        /// </remarks>
        [JsonConstructor]
        protected ServiceCatalog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCatalog"/> class
        /// with the specified name, username, and endpoints.
        /// </summary>
        /// <param name="name">The display name of the service.</param>
        /// <param name="type">The canonical name of the service.</param>
        /// <param name="endpoints">A collection of <see cref="Endpoint"/> objects describing the service endpoints.</param>
        public ServiceCatalog(string name, string type, Endpoint[] endpoints)
        {
            Name = name;
            Type = type;
            Endpoints = endpoints;
        }

        /// <summary>
        /// Gets the endpoints for the service.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("endpoints")]
        public Endpoint[] Endpoints { get; private set; }

        /// <summary>
        /// Gets the display name of the service, which may be a vendor-specific
        /// product name.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the canonical name of the specification implemented by this service.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/POST_authenticate_v2.0_tokens_.html">Authenticate (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("type")]
        public string Type { get; private set; }
    }
}