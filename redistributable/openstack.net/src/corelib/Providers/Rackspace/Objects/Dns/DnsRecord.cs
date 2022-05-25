namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class describes a single record associated with a domain in the DNS service.
    /// </summary>
    /// <seealso cref="IDnsService.ListRecordDetailsAsync"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/List_Record_Details-d1e4770.html">List Record Details (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsRecord : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
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
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private RecordId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type")]
        private DnsRecordType _type;

        /// <summary>
        /// This is the backing field for the <see cref="Comment"/> property.
        /// </summary>
        [JsonProperty("comment")]
        private string _comment;

        /// <summary>
        /// This is the backing field for the <see cref="Data"/> property.
        /// </summary>
        [JsonProperty("data")]
        private string _data;

        /// <summary>
        /// This is the backing field for the <see cref="Priority"/> property.
        /// </summary>
        [JsonProperty("priority")]
        private int? _priority;

        /// <summary>
        /// This is the backing field for the <see cref="Created"/> property.
        /// </summary>
        [JsonProperty("created")]
        private DateTimeOffset? _created;

        /// <summary>
        /// This is the backing field for the <see cref="Updated"/> property.
        /// </summary>
        [JsonProperty("updated")]
        private DateTimeOffset? _updated;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRecord"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsRecord()
        {
        }

        /// <summary>
        /// Gets the unique ID representing this record within the DNS service.
        /// </summary>
        /// <value>
        /// The unique ID for the record, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public RecordId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the DNS record type.
        /// </summary>
        /// <value>
        /// The DNS record type, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
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
        /// <value>
        /// The DNS record name, or <see langword="null"/> if the JSON response from the server
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
        /// Gets the data associated with the DNS record.
        /// </summary>
        /// <value>
        /// The DNS record data, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
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
        /// <value>
        /// The time-to-live of the DNS record, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
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
        /// Gets the timestamp when this DNS record was initially created.
        /// </summary>
        /// <value>
        /// The creation timestamp of the DNS record, or <see langword="null"/> if the JSON response from the server
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
        /// Gets the timestamp when this DNS record was last updated.
        /// </summary>
        /// <value>
        /// The last-updated timestamp of the DNS record, or <see langword="null"/> if the JSON response from the server
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
        /// Gets the priority of this DNS record.
        /// </summary>
        /// <value>
        /// The priority of the DNS record, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public int? Priority
        {
            get
            {
                return _priority;
            }
        }

        /// <summary>
        /// Gets the optional comment associated with the DNS record.
        /// </summary>
        /// <value>
        /// The comment associated with the DNS record, or <see langword="null"/> if the JSON response from the server
        /// did not include this property or if the record does not have an associated comment.
        /// </value>
        public string Comment
        {
            get
            {
                return _comment;
            }
        }
    }
}
