namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Net;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents a load balancer node configuration.
    /// </summary>
    /// <remarks>
    /// This class is used in calls to <see cref="ILoadBalancerService.AddNodeAsync"/>
    /// or <see cref="ILoadBalancerService.AddNodeRangeAsync"/>, and the
    /// <see cref="Node"/> class extends this class to represent a load balancer
    /// node that already exists as a resource in the <see cref="ILoadBalancerService"/>.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NodeConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Address"/> property.
        /// </summary>
        [JsonProperty("address")]
        private string _address;

        /// <summary>
        /// This is the backing field for the <see cref="Port"/> property.
        /// </summary>
        [JsonProperty("port")]
        private int? _port;

        /// <summary>
        /// This is the backing field for the <see cref="Condition"/> property.
        /// </summary>
        [JsonProperty("condition")]
        private NodeCondition _condition;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NodeType _type;

        /// <summary>
        /// This is the backing field for the <see cref="Weight"/> property.
        /// </summary>
        [JsonProperty("weight", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _weight;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeConfiguration"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NodeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeConfiguration"/> class.
        /// </summary>
        /// <param name="address">The IP address of the node.</param>
        /// <param name="port">The port number for the load balanced service.</param>
        /// <param name="condition">The condition for the node, which determines its role within the load balancer.</param>
        /// <param name="type">The node type. If this value is <see langword="null"/>, a provider-specific default value will be used.</param>
        /// <param name="weight">The weight of the node.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="address"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="condition"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="port"/> is less than 0 or greater than 65535.
        /// <para>-or-</para>
        /// <para>If <paramref name="weight"/> is less than or equal to 0.</para>
        /// </exception>
        public NodeConfiguration(IPAddress address, int port, NodeCondition condition, NodeType type, int? weight)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");
            if (weight <= 0)
                throw new ArgumentOutOfRangeException("weight");

            _address = address.ToString();
            _port = port;
            _condition = condition;
            _type = type;
            _weight = weight;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeConfiguration"/> class.
        /// </summary>
        /// <param name="hostDomain">The domain name of the node.</param>
        /// <param name="port">The port number for the load balanced service.</param>
        /// <param name="condition">The condition for the node, which determines its role within the load balancer.</param>
        /// <param name="type">The node type. If this value is <see langword="null"/>, a provider-specific default value will be used.</param>
        /// <param name="weight">The weight of the node.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="hostDomain"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="condition"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="hostDomain"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="port"/> is less than 0 or greater than 65535.
        /// <para>-or-</para>
        /// <para>If <paramref name="weight"/> is less than or equal to 0.</para>
        /// </exception>
        public NodeConfiguration(string hostDomain, int port, NodeCondition condition, NodeType type, int? weight)
        {
            if (hostDomain == null)
                throw new ArgumentNullException("hostDomain");
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (string.IsNullOrEmpty(hostDomain))
                throw new ArgumentException("hostDomain cannot be empty");
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");
            if (weight <= 0)
                throw new ArgumentOutOfRangeException("weight");

            _address = hostDomain;
            _port = port;
            _condition = condition;
            _type = type;
            _weight = weight;
        }

        /// <summary>
        /// Gets the IP address or domain name for the node.
        /// </summary>
        public string Address
        {
            get
            {
                return _address;
            }
        }

        /// <summary>
        /// Gets the condition for the node, which determines its role within the load balancer.
        /// </summary>
        public NodeCondition Condition
        {
            get
            {
                return _condition;
            }
        }

        /// <summary>
        /// Gets the port number of the load balanced service.
        /// </summary>
        public int? Port
        {
            get
            {
                return _port;
            }
        }

        /// <summary>
        /// Gets the load balancer node type.
        /// </summary>
        public NodeType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the weight of the load balancer node.
        /// </summary>
        /// <remarks>
        /// This property is only used by load balancers with a weighted algorithm, such as
        /// <see cref="LoadBalancingAlgorithm.WeightedRoundRobin"/>.
        /// </remarks>
        public int? Weight
        {
            get
            {
                return _weight;
            }
        }
    }
}
