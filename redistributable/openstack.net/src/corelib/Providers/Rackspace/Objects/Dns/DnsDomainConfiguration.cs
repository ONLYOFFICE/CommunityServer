namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a domain configuration for calls to <see cref="IDnsService.CreateDomainsAsync"/>.
    /// </summary>
    /// <seealso cref="IDnsService.CreateDomainsAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/create_domains.html">Create Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomainConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="RecordsList"/> property.
        /// </summary>
        [JsonProperty("recordsList")]
        private RecordsList _recordsList;

        /// <summary>
        /// This is the backing field for the <see cref="Subdomains"/> property.
        /// </summary>
        [JsonProperty("subdomains")]
        private SubdomainsList _subdomains;

        /// <summary>
        /// This is the backing field for the <see cref="EmailAddress"/> property.
        /// </summary>
        [JsonProperty("emailAddress")]
        private string _emailAddress;

        /// <summary>
        /// This is the backing field for the <see cref="TimeToLive"/> property.
        /// </summary>
        [JsonProperty("ttl")]
        private int? _timeToLive;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Comment"/> property.
        /// </summary>
        [JsonProperty("comment")]
        private string _comment;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomainConfiguration"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="name">The fully-qualified domain name.</param>
        /// <param name="timeToLive">The time-to-live for the domain.</param>
        /// <param name="emailAddress">The email address associated with the domain.</param>
        /// <param name="comment">An optional comment associated with the domain.</param>
        /// <param name="records">A collection of <see cref="DnsDomainRecordConfiguration"/> objects describing the initial DNS records to associate with the domain.</param>
        /// <param name="subdomains">A collection of <see cref="DnsSubdomainConfiguration"/> objects containing the initial subdomains to create with the domain.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="emailAddress"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="records"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="subdomains"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="records"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="subdomains"/> contains any <see langword="null"/> values.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeToLive"/> is negative or <see cref="TimeSpan.Zero"/>.</exception>
        public DnsDomainConfiguration(string name, TimeSpan? timeToLive, string emailAddress, string comment, IEnumerable<DnsDomainRecordConfiguration> records, IEnumerable<DnsSubdomainConfiguration> subdomains)
            : this(name, timeToLive, emailAddress, comment, new RecordsList(records), new SubdomainsList(subdomains))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomainConfiguration"/> class with
        /// the specified values.
        /// </summary>
        /// <remarks>
        /// <note type="inherit">
        /// Derived types may use this constructor to create a configuration with class derived from
        /// <see cref="RecordsList"/> or <see cref="SubdomainsList"/> should a server change or extension
        /// require additional information be passed in the body of the request.
        /// </note>
        /// </remarks>
        /// <param name="name">The fully-qualified domain name.</param>
        /// <param name="timeToLive">The time-to-live for the domain.</param>
        /// <param name="emailAddress">The email address associated with the domain.</param>
        /// <param name="comment">An optional comment associated with the domain.</param>
        /// <param name="records">A <see cref="RecordsList"/> object containing the initial DNS records to associate with the domain.</param>
        /// <param name="subdomains">A <see cref="SubdomainsList"/> object containing the initial subdomains to create with the domain.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="emailAddress"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="records"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="subdomains"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeToLive"/> is negative or <see cref="TimeSpan.Zero"/>.</exception>
        protected DnsDomainConfiguration(string name, TimeSpan? timeToLive, string emailAddress, string comment, RecordsList records, SubdomainsList subdomains)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (emailAddress == null)
                throw new ArgumentNullException("emailAddress");
            if (records == null)
                throw new ArgumentNullException("records");
            if (subdomains == null)
                throw new ArgumentNullException("subdomains");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentException("emailAddress cannot be empty");
            if (timeToLive <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeToLive cannot be negative or zero");

            _name = name;
            _emailAddress = emailAddress;
            _comment = comment;
            if (timeToLive.HasValue)
                _timeToLive = (int)timeToLive.Value.TotalSeconds;

            _recordsList = records;
            _subdomains = subdomains;
        }

        /// <summary>
        /// Gets the fully-qualified domain name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the email address associated with the domain.
        /// </summary>
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
        }

        /// <summary>
        /// Gets the optional comment associated with the domain.
        /// </summary>
        /// <value>
        /// The comment to associate with the domain, or <see langword="null"/> if no comment should be
        /// associated with the domain.
        /// </value>
        public string Comment
        {
            get
            {
                return _comment;
            }
        }

        /// <summary>
        /// Gets the time-to-live for the domain.
        /// </summary>
        /// <value>
        /// The time-to-live for the domain, or <see langword="null"/> if the time-to-live should be automatically
        /// assigned by the server when the domain is created.
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

        /// <summary>
        /// Gets a collection of <see cref="DnsDomainRecordConfiguration"/> objects describing the
        /// initial DNS records to create with this domain.
        /// </summary>
        public ReadOnlyCollection<DnsDomainRecordConfiguration> Records
        {
            get
            {
                return _recordsList.Records;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DnsSubdomainConfiguration"/> objects describing the
        /// initial DNS subdomains to create with this domain.
        /// </summary>
        public ReadOnlyCollection<DnsSubdomainConfiguration> Subdomains
        {
            get
            {
                return _subdomains.Subdomains;
            }
        }

        /// <summary>
        /// This class models the value of the <literal>recordsList</literal> property in the JSON
        /// request body sent to the <strong>Create Domain(s)</strong> API.
        /// </summary>
        /// <remarks>
        /// <note type="inherit">
        /// Derived types may extend this class to customize the JSON request should a server change
        /// or extension require additional information be passed in the body of the request.
        /// </note>
        /// </remarks>
        /// <seealso cref="IDnsService.CreateDomainsAsync"/>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/create_domains.html">Create Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        [JsonObject(MemberSerialization.OptIn)]
        protected class RecordsList : ExtensibleJsonObject
        {
            /// <summary>
            /// This is the backing field for the <see cref="Records"/> property.
            /// </summary>
            [JsonProperty("records")]
            private DnsDomainRecordConfiguration[] _records;

            /// <summary>
            /// Initializes a new instance of the <see cref="RecordsList"/> class with the
            /// specified DNS record configurations.
            /// </summary>
            /// <param name="records">A collection of <see cref="DnsDomainRecordConfiguration"/> objects describing the DNS records to associate with a domain.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="records"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">If <paramref name="records"/> contains any <see langword="null"/> values.</exception>
            protected internal RecordsList(IEnumerable<DnsDomainRecordConfiguration> records)
            {
                if (records == null)
                    throw new ArgumentNullException("records");

                _records = records.ToArray();
                if (_records.Contains(null))
                    throw new ArgumentException("records cannot contain any null values.");
            }

            /// <summary>
            /// Gets a collection of <see cref="DnsDomainRecordConfiguration"/> objects describing the DNS records to associate with a domain.
            /// </summary>
            public ReadOnlyCollection<DnsDomainRecordConfiguration> Records
            {
                get
                {
                    return new ReadOnlyCollection<DnsDomainRecordConfiguration>(_records);
                }
            }
        }

        /// <summary>
        /// This class models the value of the <literal>subdomains</literal> property in the JSON
        /// request body sent to the <strong>Create Domain(s)</strong> API.
        /// </summary>
        /// <remarks>
        /// <note type="inherit">
        /// Derived types may extend this class to customize the JSON request should a server change
        /// or extension require additional information be passed in the body of the request.
        /// </note>
        /// </remarks>
        /// <seealso cref="IDnsService.CreateDomainsAsync"/>
        /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/create_domains.html">Create Domain(s) (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        [JsonObject(MemberSerialization.OptIn)]
        protected class SubdomainsList : ExtensibleJsonObject
        {
            /// <summary>
            /// This is the backing field for the <see cref="Subdomains"/> property.
            /// </summary>
            [JsonProperty("domains")]
            private DnsSubdomainConfiguration[] _subdomains;

            /// <summary>
            /// Initializes a new instance of the <see cref="SubdomainsList"/> class with the
            /// specified subdomain configurations.
            /// </summary>
            /// <param name="subdomains">A collection of <see cref="DnsSubdomainConfiguration"/> objects describing the subdomains to associate with a domain.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="subdomains"/> is <see langword="null"/>.</exception>
            /// <exception cref="ArgumentException">If <paramref name="subdomains"/> contains any <see langword="null"/> values.</exception>
            protected internal SubdomainsList(IEnumerable<DnsSubdomainConfiguration> subdomains)
            {
                if (subdomains == null)
                    throw new ArgumentNullException("subdomains");

                _subdomains = subdomains.ToArray();
                if (_subdomains.Contains(null))
                    throw new ArgumentException("subdomains cannot contain any null values.");
            }

            /// <summary>
            /// Gets a collection of <see cref="DnsSubdomainConfiguration"/> objects describing the subdomains to associate with a domain.
            /// </summary>
            public ReadOnlyCollection<DnsSubdomainConfiguration> Subdomains
            {
                get
                {
                    return new ReadOnlyCollection<DnsSubdomainConfiguration>(_subdomains);
                }
            }
        }
    }
}
