namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the object associated with the <c>"subdomains"</c> property in
    /// the JSON model for a DNS domain.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsSubdomainsList : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Subdomains"/> property.
        /// </summary>
        [JsonProperty("domains")]
        private DnsSubdomain[] _subdomains;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsSubdomainsList"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsSubdomainsList()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsSubdomain"/> objects describing the subdomains
        /// associated with a <see cref="DnsDomain"/>.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsSubdomain"/> objects, or <see langword="null"/> if the JSON response
        /// from the server did not include the associated property.
        /// </value>
        public ReadOnlyCollection<DnsSubdomain> Subdomains
        {
            get
            {
                if (_subdomains == null)
                    return null;

                return new ReadOnlyCollection<DnsSubdomain>(_subdomains);
            }
        }
    }
}
