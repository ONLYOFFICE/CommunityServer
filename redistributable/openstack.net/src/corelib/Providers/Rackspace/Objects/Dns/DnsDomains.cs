namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a collection of DNS domains returned from a call to create, clone, or
    /// import domains in the DNS service.
    /// </summary>
    /// <seealso cref="IDnsService.CreateDomainsAsync"/>
    /// <seealso cref="IDnsService.CloneDomainAsync"/>
    /// <seealso cref="IDnsService.ImportDomainAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/create_domains.html">Create Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/clone_domain-dle846.html">Clone Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/import_domain.html">Import Domain (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomains : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Domains"/> property.
        /// </summary>
        [JsonProperty("domains")]
        private DnsDomain[] _domains;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomains"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsDomains()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsDomain"/> objects describing domains in the DNS service.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsDomain"/> objects describing domains in the DNS service, or
        /// <see langword="null"/> if the JSON response from the server did not include this property.
        /// </value>
        public ReadOnlyCollection<DnsDomain> Domains
        {
            get
            {
                if (_domains == null)
                    return null;

                return new ReadOnlyCollection<DnsDomain>(_domains);
            }
        }
    }
}
