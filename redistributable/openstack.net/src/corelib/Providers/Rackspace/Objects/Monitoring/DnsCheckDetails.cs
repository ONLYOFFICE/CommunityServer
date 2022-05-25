namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using DnsRecordType = net.openstack.Providers.Rackspace.Objects.Dns.DnsRecordType;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteDns"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteDns"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsCheckDetails : ConnectionCheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Query"/> property.
        /// </summary>
        [JsonProperty("query")]
        private string _query;

        /// <summary>
        /// This is the backing field for the <see cref="RecordType"/> property.
        /// </summary>
        [JsonProperty("record_type")]
        private DnsRecordType _recordType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsCheckDetails"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="query">The DNS query.</param>
        /// <param name="recordType">The DNS record type.</param>
        /// <param name="port">The port number of the DNS service. If this value is <see langword="null"/>, the default value (53) for the service is used.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="query"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="recordType"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public DnsCheckDetails(string query, DnsRecordType recordType, int? port = null)
            : base(port)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("query cannot be empty");
            if (recordType == null)
                throw new ArgumentNullException("recordType");

            _query = query;
            _recordType = recordType;
        }

        /// <summary>
        /// Gets the DNS query.
        /// </summary>
        public string Query
        {
            get
            {
                return _query;
            }
        }

        /// <summary>
        /// Gets the DNS record type.
        /// </summary>
        public DnsRecordType RecordType
        {
            get
            {
                return _recordType;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteDns"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteDns;
        }
    }
}
