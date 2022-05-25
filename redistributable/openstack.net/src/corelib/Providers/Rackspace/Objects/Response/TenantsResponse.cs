namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Tenants request.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-identity-service/2.0/content/GET_listTenants_v2.0_tenants_Tenant_Operations.html">List Tenants (OpenStack Identity Service API v2.0 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class TenantsResponse
    {
        /// <summary>
        /// Gets a collection of information about the tenants.
        /// </summary>
        [JsonProperty("tenants")]
        public Tenant[] Tenants { get; private set; }
    }
}
