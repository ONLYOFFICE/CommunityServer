namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using Newtonsoft.Json;
    using ProjectId = net.openstack.Core.Domain.ProjectId;
    using Tenant = net.openstack.Core.Domain.Tenant;

    /// <summary>
    /// Extends the <see cref="SerializedDomain"/> object to expose additional information
    /// provided by the <see cref="IDnsService.ExportDomainAsync"/> method.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/export_domain.html">Export Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ExportedDomain : SerializedDomain
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="AccountId"/> property.
        /// </summary>
        [JsonProperty("accountId")]
        private ProjectId _accountId;

        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private DomainId _id;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportedDomain"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ExportedDomain()
        {
        }

        /// <summary>
        /// Gets the ID of the exported domain.
        /// </summary>
        /// <value>
        /// The ID for the exported domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        /// <seealso cref="DnsDomain.Id"/>
        public DomainId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the account ID associated with this domain. The account ID within the DNS service
        /// is equivalent to the <see cref="Tenant.Id">Tenant.Id</see> referenced by other services.
        /// </summary>
        /// <value>
        /// The account ID for the domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public ProjectId AccountId
        {
            get
            {
                return _accountId;
            }
        }
    }
}
