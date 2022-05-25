namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class provides information about load balancer usage during a span of time.
    /// </summary>
    /// <seealso cref="ILoadBalancerService.ListAccountLevelUsageAsync"/>
    /// <seealso cref="ILoadBalancerService.ListHistoricalUsageAsync"/>
    /// <seealso cref="ILoadBalancerService.ListCurrentUsageAsync"/>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerUsage : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private LoadBalancerUsageId _id;

        /// <summary>
        /// This is the backing field for the <see cref="AverageConnections"/> property.
        /// </summary>
        [JsonProperty("averageNumConnections")]
        private double? _averageConnections;

        /// <summary>
        /// This is the backing field for the <see cref="IncomingTransfer"/> property.
        /// </summary>
        [JsonProperty("incomingTransfer")]
        private long? _incomingTransfer;

        /// <summary>
        /// This is the backing field for the <see cref="OutgoingTransfer"/> property.
        /// </summary>
        [JsonProperty("outgoingTransfer")]
        private long? _outgoingTransfer;

        /// <summary>
        /// This is the backing field for the <see cref="AverageConnectionsSsl"/> property.
        /// </summary>
        [JsonProperty("averageNumConnectionsSsl")]
        private double? _averageConnectionsSsl;

        /// <summary>
        /// This is the backing field for the <see cref="IncomingTransferSsl"/> property.
        /// </summary>
        [JsonProperty("incomingTransferSsl")]
        private long? _incomingTransferSsl;

        /// <summary>
        /// This is the backing field for the <see cref="OutgoingTransferSsl"/> property.
        /// </summary>
        [JsonProperty("outgoingTransferSsl")]
        private long? _outgoingTransferSsl;

        /// <summary>
        /// This is the backing field for the <see cref="VirtualAddressCount"/> property.
        /// </summary>
        [JsonProperty("numVips")]
        private int? _virtualAddressCount;

        /// <summary>
        /// This is the backing field for the <see cref="PollCount"/> property.
        /// </summary>
        [JsonProperty("numPolls")]
        private int? _pollCount;

        /// <summary>
        /// This is the backing field for the <see cref="StartTime"/> property.
        /// </summary>
        [JsonProperty("startTime")]
        private DateTimeOffset? _startTime;

        /// <summary>
        /// This is the backing field for the <see cref="EndTime"/> property.
        /// </summary>
        [JsonProperty("endTime")]
        private DateTimeOffset? _endTime;

        /// <summary>
        /// This is the backing field for the <see cref="VirtualAddressType"/> property.
        /// </summary>
        [JsonProperty("vipType")]
        private LoadBalancerVirtualAddressType _virtualAddressType;

        /// <summary>
        /// This is the backing field for the <see cref="SslMode"/> property.
        /// </summary>
        [JsonProperty("sslMode")]
        private string _sslMode;

        /// <summary>
        /// This is the backing field for the <see cref="EventType"/> property.
        /// </summary>
        [JsonProperty("eventType")]
        private string _eventType;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerUsage"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerUsage()
        {
        }

        /// <summary>
        /// Gets the unique ID for this usage record.
        /// </summary>
        public LoadBalancerUsageId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the average number of connections open to the load balancer between <see cref="StartTime"/> and <see cref="EndTime"/>.
        /// </summary>
        public double? AverageConnections
        {
            get
            {
                return _averageConnections;
            }
        }

        /// <summary>
        /// Gets the number of incoming bytes transferred.
        /// </summary>
        public long? IncomingTransfer
        {
            get
            {
                return _incomingTransfer;
            }
        }

        /// <summary>
        /// Gets the number of outgoing bytes transferred.
        /// </summary>
        public long? OutgoingTransfer
        {
            get
            {
                return _outgoingTransfer;
            }
        }

        /// <summary>
        /// Gets the average number of secure connections open to the load balancer between <see cref="StartTime"/> and <see cref="EndTime"/>.
        /// </summary>
        public double? AverageConnectionsSsl
        {
            get
            {
                return _averageConnectionsSsl;
            }
        }

        /// <summary>
        /// Gets the number of incoming bytes transferred over secure connections.
        /// </summary>
        public long? IncomingTransferSsl
        {
            get
            {
                return _incomingTransferSsl;
            }
        }

        /// <summary>
        /// Gets the number of outgoing bytes transferred over secure connections.
        /// </summary>
        public long? OutgoingTransferSsl
        {
            get
            {
                return _outgoingTransferSsl;
            }
        }

        /// <summary>
        /// Gets the number of virtual addresses assigned to the load balancer between <see cref="StartTime"/> and <see cref="EndTime"/>.
        /// </summary>
        public int? VirtualAddressCount
        {
            get
            {
                return _virtualAddressCount;
            }
        }

        /// <summary>
        /// Gets <placeholder>what?</placeholder>
        /// </summary>
        public int? PollCount
        {
            get
            {
                return _pollCount;
            }
        }

        /// <summary>
        /// Gets the starting timestamp for this load balancer usage record.
        /// </summary>
        public DateTimeOffset? StartTime
        {
            get
            {
                return _startTime;
            }
        }

        /// <summary>
        /// Gets the ending timestamp for this load balancer usage record.
        /// </summary>
        public DateTimeOffset? EndTime
        {
            get
            {
                return _endTime;
            }
        }

        /// <summary>
        /// Gets the virtual address type for the load balancer between <see cref="StartTime"/> and <see cref="EndTime"/>.
        /// </summary>
        public LoadBalancerVirtualAddressType VirtualAddressType
        {
            get
            {
                return _virtualAddressType;
            }
        }

        /// <summary>
        /// Gets the SSL mode for the load balancer between <see cref="StartTime"/> and <see cref="EndTime"/>.
        /// </summary>
        public string SslMode
        {
            get
            {
                return _sslMode;
            }
        }

        /// <summary>
        /// Gets <placeholder>what?</placeholder>
        /// </summary>
        public string EventType
        {
            get
            {
                return _eventType;
            }
        }
    }
}
