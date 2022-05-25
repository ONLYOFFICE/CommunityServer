namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents the updates to apply to a collection of domains in the DNS service.
    /// </summary>
    /// <remarks>
    /// <note type="inherit">
    /// This class can be extended if a server extension requires additional information (beyond
    /// the <c>domains</c> property which is already supported) be sent in the body of a
    /// <strong>Modify Domain(s)</strong> API call.
    /// </note>
    /// </remarks>
    /// <seealso cref="IDnsService.UpdateDomainsAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsUpdateConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="DomainConfigurations"/> property.
        /// </summary>
        [JsonProperty("domains")]
        private readonly DnsDomainUpdateConfiguration[] _domainConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsUpdateConfiguration"/> class for the
        /// specified domains.
        /// </summary>
        /// <param name="domainConfigurations">A collection of <see cref="DnsDomainUpdateConfiguration"/> objects describing the domains.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="domainConfigurations"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="domainConfigurations"/> contains a <see langword="null"/> value.</exception>
        public DnsUpdateConfiguration(params DnsDomainUpdateConfiguration[] domainConfigurations)
            : this(domainConfigurations.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsUpdateConfiguration"/> class for the
        /// specified domains.
        /// </summary>
        /// <param name="domainConfigurations">A collection of <see cref="DnsDomainUpdateConfiguration"/> objects describing the domains.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="domainConfigurations"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="domainConfigurations"/> contains a <see langword="null"/> value.</exception>
        public DnsUpdateConfiguration(IEnumerable<DnsDomainUpdateConfiguration> domainConfigurations)
        {
            if (domainConfigurations == null)
                throw new ArgumentNullException("domainConfigurations");

            _domainConfiguration = domainConfigurations.ToArray();

            if (_domainConfiguration.Contains(null))
                throw new ArgumentException("domainConfigurations cannot contain any null values.", "domainConfigurations");
        }

        /// <summary>
        /// Gets a collection of the <see cref="DnsDomainUpdateConfiguration"/> objects describing
        /// domains in this configuration.
        /// </summary>
        public ReadOnlyCollection<DnsDomainUpdateConfiguration> DomainConfigurations
        {
            get
            {
                return new ReadOnlyCollection<DnsDomainUpdateConfiguration>(_domainConfiguration);
            }
        }
    }
}
