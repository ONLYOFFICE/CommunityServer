namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents the load balancer statistics returned from a
    /// call to <see cref="ILoadBalancerService.GetStatisticsAsync"/>.
    /// </summary>
    /// <seealso cref="ILoadBalancerService.GetStatisticsAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerStatistics : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        [JsonProperty("connectTimeOut", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _connectTimeOut;

        [JsonProperty("connectError", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _connectError;

        [JsonProperty("connectFailure", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _connectFailure;

        [JsonProperty("dataTimedOut", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _dataTimedOut;

        [JsonProperty("keepAliveTimedOut", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _keepAliveTimedOut;

        [JsonProperty("maxConn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _maxConn;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerStatistics"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerStatistics()
        {
        }

        /// <summary>
        /// Gets the number of connections closed by the load balancer because the <em>connect_timeout</em> interval was exceeded.
        /// </summary>
        public long? ConnectionTimedOut
        {
            get
            {
                return _connectTimeOut;
            }
        }

        /// <summary>
        /// Gets the number of transaction or protocol errors in the load balancer.
        /// </summary>
        public long? ConnectionError
        {
            get
            {
                return _connectError;
            }
        }

        /// <summary>
        /// Gets the number of connection failures in the load balancer.
        /// </summary>
        public long? ConnectionFailure
        {
            get
            {
                return _connectFailure;
            }
        }

        /// <summary>
        /// Gets the number of connections closed by this load balancer because the <em>timeout</em> interval was exceeded.
        /// </summary>
        public long? DataTimedOut
        {
            get
            {
                return _dataTimedOut;
            }
        }

        /// <summary>
        /// Gets the number of connections closed by this load balancer because the <em>keepalive_timeout</em> interval was exceeded.
        /// </summary>
        public long? KeepAliveTimedOut
        {
            get
            {
                return _keepAliveTimedOut;
            }
        }

        /// <summary>
        /// Gets the maximum number of simultaneous TCP connections this load balancer has processed at any one time.
        /// </summary>
        public long? MaxConnections
        {
            get
            {
                return _maxConn;
            }
        }
    }
}
