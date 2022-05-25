namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This models a DNS record for the <see cref="IDnsService.CreateDomainsAsync"/>
    /// and <see cref="IDnsService.AddRecordsAsync"/> calls.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/supported_record_types.html">Supported Record Types (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Add_Records-d1e4895.html">Add Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomainRecordConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type")]
        private DnsRecordType _type;

        /// <summary>
        /// This is the backing field for the <see cref="Data"/> property.
        /// </summary>
        [JsonProperty("data")]
        private string _data;

        /// <summary>
        /// This is the backing field for the <see cref="TimeToLive"/> property.
        /// </summary>
        [JsonProperty("ttl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _timeToLive;

        /// <summary>
        /// This is the backing field for the <see cref="Comment"/> property.
        /// </summary>
        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _comment;

        /// <summary>
        /// This is the backing field for the <see cref="Priority"/> property.
        /// </summary>
        [JsonProperty("priority", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDomainRecordConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="type">The DNS record type.</param>
        /// <param name="name">The DNS record name.</param>
        /// <param name="data">The data to associate with the DNS record.</param>
        /// <param name="timeToLive">The time-to-live for the DNS record. If not specified, a provider-specific default value will be used.</param>
        /// <param name="comment">An optional comment to associate with the DNS record.</param>
        /// <param name="priority">The priority of the DNS record. This is only specified for <see cref="DnsRecordType.Mx"/> and <see cref="DnsRecordType.Srv"/> records.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="name"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="data"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="data"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="priority"/> is specified and <paramref name="type"/> is <em>not</em> <see cref="DnsRecordType.Mx"/> or <see cref="DnsRecordType.Srv"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="priority"/> is <em>not</em> specified and <paramref name="type"/> is <see cref="DnsRecordType.Mx"/> or <see cref="DnsRecordType.Srv"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="timeToLive"/> is negative.
        /// <para>-or-</para>
        /// <para>If <paramref name="priority"/> is less than 0.</para>
        /// </exception>
        public DnsDomainRecordConfiguration(DnsRecordType type, string name, string data, TimeSpan? timeToLive, string comment, int? priority)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (name == null)
                throw new ArgumentNullException("name");
            if (data == null)
                throw new ArgumentNullException("data");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("data cannot be empty");
            if (timeToLive <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeToLive cannot be negative or zero");
            if (priority < 0)
                throw new ArgumentOutOfRangeException("priority");

            if (type == DnsRecordType.Mx || type == DnsRecordType.Srv)
            {
                if (!priority.HasValue)
                    throw new ArgumentException("A priority must be specified for MX and SRV records.");
            }
            else
            {
                if (priority.HasValue)
                    throw new ArgumentException(string.Format("A priority cannot be specified for {0} records.", type));
            }

            _name = name;
            _type = type;
            _data = data;
            _comment = comment;
            _priority = priority;
            if (timeToLive != null)
                _timeToLive = (int)timeToLive.Value.TotalSeconds;
        }

        /// <summary>
        /// Gets the DNS record type.
        /// </summary>
        public DnsRecordType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the name of the DNS record.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the data for this DNS record.
        /// </summary>
        public string Data
        {
            get
            {
                return _data;
            }
        }

        /// <summary>
        /// Gets the time-to-live for the DNS record.
        /// </summary>
        public TimeSpan? TimeToLive
        {
            get
            {
                if (!_timeToLive.HasValue)
                    return null;

                return TimeSpan.FromSeconds(_timeToLive.Value);
            }
        }

        /// <summary>
        /// Gets the optional comment associated with the DNS record.
        /// </summary>
        public string Comment
        {
            get
            {
                return _comment;
            }
        }

        /// <summary>
        /// Gets the priority associated with the DNS record.
        /// </summary>
        /// <remarks>
        /// The priority only applies to <see cref="DnsRecordType.Mx"/> and <see cref="DnsRecordType.Srv"/>
        /// records. For other record types, this property is <see langword="null"/>.
        /// </remarks>
        public int? Priority
        {
            get
            {
                return _priority;
            }
        }
    }
}
