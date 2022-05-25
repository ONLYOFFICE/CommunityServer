namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the basic JSON description of a tenant.
    /// </summary>
    /// <seealso cref="IIdentityProvider.ListTenants"/>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tenant : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the unique identifier for the tenant.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the "name" property for the tenant.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}
