namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This models a DNS record configuration update for the <see cref="IDnsService.UpdateRecordsAsync"/> call.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/supported_record_types.html">Supported Record Types (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/Modify_Records-d1e5033.html">Modify Records (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsDomainRecordUpdateConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private RecordId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Data"/> property.
        /// </summary>
        [JsonProperty("data", DefaultValueHandling = DefaultValueHandling.Ignore)]
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
        /// Initializes a new instance of the <see cref="DnsDomainRecordUpdateConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="record">The DNS record to update.</param>
        /// <param name="name">The DNS record name.</param>
        /// <param name="data">The data to associate with the DNS record. If this value is <see langword="null"/>, the existing value for the record is not changed.</param>
        /// <param name="timeToLive">The time-to-live for the DNS record. If this value is <see langword="null"/>, the existing value for the record is not changed.</param>
        /// <param name="comment">An optional comment to associate with the DNS record. If this value is <see langword="null"/>, the existing value for the record is not changed.</param>
        /// <param name="priority">The priority of the DNS record. This is only specified for <see cref="DnsRecordType.Mx"/> and <see cref="DnsRecordType.Srv"/> records. If this value is <see langword="null"/>, the existing value for the record is not changed.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="record"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="name"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="data"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="priority"/> is specified and the type of <paramref name="record"/> is <em>not</em> <see cref="DnsRecordType.Mx"/> or <see cref="DnsRecordType.Srv"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="timeToLive"/> is negative.
        /// <para>-or-</para>
        /// <para>If <paramref name="priority"/> is less than 0.</para>
        /// </exception>
        public DnsDomainRecordUpdateConfiguration(DnsRecord record, string name, string data = null, TimeSpan? timeToLive = null, string comment = null, int? priority = null)
        {
            if (record == null)
                throw new ArgumentNullException("record");
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (data == string.Empty)
                throw new ArgumentException("data cannot be empty");
            if (timeToLive <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeToLive cannot be negative or zero");
            if (priority < 0)
                throw new ArgumentOutOfRangeException("priority");

            if (record.Type != DnsRecordType.Mx && record.Type != DnsRecordType.Srv)
            {
                if (priority.HasValue)
                    throw new ArgumentException(string.Format("A priority cannot be specified for {0} records.", record.Type));
            }

            _id = record.Id;
            _name = name;
            _data = data;
            _comment = comment;
            _priority = priority;
            if (timeToLive != null)
                _timeToLive = (int)timeToLive.Value.TotalSeconds;
        }

        /// <summary>
        /// Gets the unique ID of the record to update within the DNS service.
        /// </summary>
        /// <value>
        /// The unique ID for the record.
        /// </value>
        public RecordId Id
        {
            get
            {
                return _id;
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
