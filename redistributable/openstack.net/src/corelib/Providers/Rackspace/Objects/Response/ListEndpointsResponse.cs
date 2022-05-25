using net.openstack.Core.Domain;
using Newtonsoft.Json;

namespace net.openstack.Providers.Rackspace.Objects.Response
{
    /// <summary>
    /// This models the JSON response used for the List Endpoints request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listEndpointsForToken_v2.0_tokens__tokenId__endpoints_Token_Operations.html">List Token Endpoints (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listEndpoints__v2.0_tenants__tenantId__OS-KSCATALOG_endpoints_Endpoint_Operations_OS-KSCATALOG.html">List Service Catalog Endpoints (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListEndpointsResponse
    {
        /// <summary>
        /// Gets additional information about the endpoints.
        /// </summary>
        /// <seealso cref="UserAccess"/>
        [JsonProperty("endpoints", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ExtendedEndpoint[] Endpoints { get; private set; }
    }
}
