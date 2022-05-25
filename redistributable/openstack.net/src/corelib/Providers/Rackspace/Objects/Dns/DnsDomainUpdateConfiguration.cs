namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a domain within the DNS service.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomainUpdateConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="EmailAddress"/> property.
        /// </summary>
        [JsonProperty("emailAddress")]
        private string _emailAddress;

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
        /// This is the backing field for the <see cref="TimeToLive"/> property.
        /// </summary>
        [JsonProperty("ttl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _timeToLive;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomainUpdateConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsDomainUpdateConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomainUpdateConfiguration"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="domain">The domain to update.</param>
        /// <param name="timeToLive">The time-to-live for the domain. If this value is <see langword="null"/>, the existing value for the domain is not updated.</param>
        /// <param name="emailAddress">The email address associated with the domain. If this value is <see langword="null"/>, the existing value for the domain is not updated.</param>
        /// <param name="comment">An optional comment associated with the domain. If this value is <see langword="null"/>, the existing value for the domain is not updated.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="domain"/> is <see langword="null"/>.</exception>
        public DnsDomainUpdateConfiguration(DnsDomain domain, string emailAddress = null, string comment = null, TimeSpan? timeToLive = null)
        {
            if (domain == null)
                throw new ArgumentNullException("domain");

            _id = domain.Id;
            _emailAddress = emailAddress;
            _comment = comment;
            _timeToLive = timeToLive.HasValue ? (int?)timeToLive.Value.TotalSeconds : null;
        }

        /// <summary>
        /// Gets the unique ID representing this domain within the DNS service.
        /// </summary>
        /// <value>
        /// The unique ID for the domain.
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
        /// The comment for the domain, or <see langword="null"/> if
        /// the current comment for the domain should not be changed.
        /// </value>
        public string Comment
        {
            get
            {
                return _comment;
            }
        }

        /// <summary>
        /// Gets the email address associated with this domain.
        /// </summary>
        /// <value>
        /// The email address for the domain, or <see langword="null"/> if the current email address for the
        /// domain should not be changed.
        /// </value>
        public string EmailAddress
        {
            get
            {
                return _emailAddress;
            }
        }

        /// <summary>
        /// Gets the time-to-live for the domain.
        /// </summary>
        /// <value>
        /// A <see cref="TimeSpan"/> object containing the time-to-live for the domain, or <see langword="null"/> if
        /// the current time-to-live for the domain should not be changed.
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
