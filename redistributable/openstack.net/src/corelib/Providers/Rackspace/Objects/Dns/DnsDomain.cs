namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.ObjectModel;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a domain within the DNS service.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomain : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="EmailAddress"/> property.
        /// </summary>
        [JsonProperty("emailAddress")]
        private string _emailAddress;

        /// <summary>
        /// This is the backing field for the <see cref="Updated"/> property.
        /// </summary>
        [JsonProperty("updated")]
        private DateTimeOffset? _updated;

        /// <summary>
        /// This is the backing field for the <see cref="Created"/> property.
        /// </summary>
        [JsonProperty("created")]
        private DateTimeOffset? _created;

        /// <summary>
        /// This is the backing field for the <see cref="AccountId"/> property.
        /// </summary>
        [JsonProperty("accountId")]
        private ProjectId _accountId;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private DomainId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Comment"/> property.
        /// </summary>
        [JsonProperty("comment")]
        private string _comment;

        /// <summary>
        /// This is the backing field for the <see cref="Nameservers"/> property.
        /// </summary>
        [JsonProperty("nameservers")]
        private DnsNameserver[] _nameservers;

        /// <summary>
        /// This is the backing field for the <see cref="TimeToLive"/> property.
        /// </summary>
        [JsonProperty("ttl")]
        private int? _timeToLive;

        /// <summary>
        /// This is an intermediate representation for the <see cref="Records"/> property,
        /// which is necessary for correctly modeling the JSON response from the server.
        /// </summary>
        [JsonProperty("recordsList")]
        private DnsRecordsList _recordsList;

        /// <summary>
        /// This is an intermediate representation for the <see cref="Subdomains"/> property,
        /// which is necessary for correctly modeling the JSON response from the server.
        /// </summary>
        [JsonProperty("subdomains")]
        private DnsSubdomainsList _subdomains;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomain"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsDomain()
        {
        }

        /// <summary>
        /// Gets the domain name.
        /// </summary>
        /// <value>
        /// The name of the domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the unique ID representing this domain within the DNS service.
        /// </summary>
        /// <value>
        /// The unique ID for the domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public DomainId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the comment associated with this domain.
        /// </summary>
        /// <value>
        /// The comment for the domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public string Comment
        {
            get
            {
                return _comment;
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

        /// <summary>
        /// Gets the email address associated with this domain.
        /// </summary>
        /// <value>
        /// The email address for the domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
        }

        /// <summary>
        /// Gets the timestamp when this domain was first added to the DNS service.
        /// </summary>
        /// <value>
        /// The creation timestamp for the domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public DateTimeOffset? Created
        {
            get
            {
                return _created;
            }
        }

        /// <summary>
        /// Gets the timestamp when this domain was last updated within the DNS service.
        /// </summary>
        /// <value>
        /// The last-updated timestamp for the domain, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public DateTimeOffset? Updated
        {
            get
            {
                return _updated;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsRecord"/> objects describing the DNS records for the domain.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsRecord"/> objects, or <see langword="null"/> if the JSON response from the
        /// server did not include the associated property.
        /// </value>
        public ReadOnlyCollection<DnsRecord> Records
        {
            get
            {
                if (_recordsList == null)
                    return null;

                return _recordsList.Records;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsSubdomain"/> objects describing the DNS subdomains for the domain.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsSubdomain"/> objects, or <see langword="null"/> if the JSON response from the
        /// server did not include the associated property.
        /// </value>
        public ReadOnlyCollection<DnsSubdomain> Subdomains
        {
            get
            {
                if (_subdomains == null)
                    return null;

                return _subdomains.Subdomains;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsNameserver"/> objects describing the DNS nameservers for the domain.
        /// </summary>
        /// <value>
        /// A collection of <see cref="DnsNameserver"/> objects, or <see langword="null"/> if the JSON response from the
        /// server did not include the associated property.
        /// </value>
        public ReadOnlyCollection<DnsNameserver> Nameservers
        {
            get
            {
                if (_nameservers == null)
                    return null;

                return new ReadOnlyCollection<DnsNameserver>(_nameservers);
            }
        }

        /// <summary>
        /// Gets the time-to-live for the domain.
        /// </summary>
        /// <value>
        /// A <see cref="TimeSpan"/> object containing the time-to-live for the domain, or <see langword="null"/> if the
        /// JSON response from the server did not include the associated property.
        /// </value>
        public TimeSpan? TimeToLive
        {
            get
            {
                if (_timeToLive == null)
                    return null;

                return TimeSpan.FromSeconds(_timeToLive.Value);
            }
        }
    }
}
