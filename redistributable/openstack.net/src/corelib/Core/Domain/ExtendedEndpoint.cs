using System.Diagnostics;
using Newtonsoft.Json;

namespace net.openstack.Core.Domain
{
    /// <summary>
    /// Represents an endpoint for a tenant that is returned outside of the <see cref="ServiceCatalog"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listEndpointsForToken_v2.0_tokens__tokenId__endpoints_Token_Operations.html">List Token Endpoints (OpenStack Identity Service API v2.0 Reference)</seealso>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Name,nq} ({Type,nq})")]
    public class ExtendedEndpoint : Endpoint
    {
        /// <summary>
        /// Gets the id of the endpoint, which may be a vendor-specific id.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listEndpointsForToken_v2.0_tokens__tokenId__endpoints_Token_Operations.html">List Token Endpoints (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the display name of the service, which may be a vendor-specific
        /// product name.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listEndpointsForToken_v2.0_tokens__tokenId__endpoints_Token_Operations.html">List Token Endpoints (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the canonical name of the specification implemented by this service.
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listEndpointsForToken_v2.0_tokens__tokenId__endpoints_Token_Operations.html">List Token Endpoint (OpenStack Identity Service API v2.0 Reference)</seealso>
        [JsonProperty("type")]
        public string Type { get; private set; }
    }
}