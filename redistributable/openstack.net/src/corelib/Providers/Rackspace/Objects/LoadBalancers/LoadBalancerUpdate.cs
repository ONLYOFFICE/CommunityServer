namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This object models the JSON representation of a configuration update to apply
    /// to an existing load balancer.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerUpdate : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="ProtocolName"/> property.
        /// </summary>
        [JsonProperty("protocol", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _protocolName;

        /// <summary>
        /// This is the backing field for the <see cref="HalfClosed"/> property.
        /// </summary>
        [JsonProperty("halfClosed", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private bool? _halfClosed;

        /// <summary>
        /// This is the backing field for the <see cref="Algorithm"/> property.
        /// </summary>
        [JsonProperty("algorithm", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancingAlgorithm _algorithm;

        /// <summary>
        /// This is the backing field for the <see cref="Port"/> property.
        /// </summary>
        [JsonProperty("port", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _port;

        /// <summary>
        /// This is the backing field for the <see cref="Timeout"/> property.
        /// </summary>
        [JsonProperty("timeout", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerUpdate"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerUpdate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerUpdate"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="name">The name of the load balancer. If this value is <see langword="null"/>, the existing value for the load balancer is not changed.</param>
        /// <param name="protocol">The load balancing protocol to use for this load balancer. If this value is <see langword="null"/>, the existing value for the load balancer is not changed.</param>
        /// <param name="halfClosed"><see langword="true"/> to enable half-closed support for the load balancer; otherwise, <see langword="false"/>. Half-Closed support provides the ability for one end of the connection to terminate its output, while still receiving data from the other end. Only applies to TCP/TCP_CLIENT_FIRST protocols. If this value is <see langword="null"/>, the existing value for the load balancer is not changed.</param>
        /// <param name="algorithm">The load balancing algorithm that defines how traffic should be directed between back-end nodes. If this value is <see langword="null"/>, the existing value for the load balancer is not changed.</param>
        /// <param name="timeout">The timeout value for the load balancer and communications with its nodes. If this value is <see langword="null"/>, the existing value for the load balancer is not changed.</param>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="timeout"/> is negative or <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public LoadBalancerUpdate(string name = null, LoadBalancingProtocol protocol = null, bool? halfClosed = null, LoadBalancingAlgorithm algorithm = null, TimeSpan? timeout = null)
        {
            if (name == string.Empty)
                throw new ArgumentException("name cannot be empty");
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeout");

            _name = name;
            _protocolName = protocol != null ? protocol.Name : null;
            _port = protocol != null ? (int?)protocol.Port : null;
            _halfClosed = halfClosed;
            _algorithm = algorithm;
            _timeout = timeout != null ? (int?)timeout.Value.TotalSeconds : null;
        }

        /// <summary>
        /// Gets the name of the load balancer.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the name of the load balanced protocol.
        /// </summary>
        public string ProtocolName
        {
            get
            {
                return _protocolName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not half-closed support is enabled for the load balancer.
        /// </summary>
        public bool? HalfClosed
        {
            get
            {
                return _halfClosed;
            }
        }

        /// <summary>
        /// Gets the load balancing algorithm used for distributing data between back-end nodes.
        /// </summary>
        public LoadBalancingAlgorithm Algorithm
        {
            get
            {
                return _algorithm;
            }
        }

        /// <summary>
        /// Gets the port number the load balancer will listen for connections on.
        /// </summary>
        public int? Port
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// Gets the timeout value for the load balancer and communications with its nodes.
        /// </summary>
        public TimeSpan? Timeout
        {
            get
            {
                if (_timeout == null)
                    return null;

                return TimeSpan.FromSeconds(_timeout.Value);
            }
        }
    }
}
