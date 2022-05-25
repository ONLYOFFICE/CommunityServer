namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents a load balancer configuration.
    /// </summary>
    /// <remarks>
    /// The <see cref="LoadBalancerConfiguration"/> extends this class for use in calls to
    /// <see cref="ILoadBalancerService.CreateLoadBalancerAsync"/>, and the
    /// <see cref="LoadBalancer"/> class extends this class to represent a load balancer
    /// that already exists as a resource in the <see cref="ILoadBalancerService"/>.
    /// </remarks>
    /// <typeparam name="TNodeConfiguration">The class used to represent a load balancer node configuration.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerConfiguration<TNodeConfiguration> : ExtensibleJsonObject
        where TNodeConfiguration : NodeConfiguration
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Nodes"/> property.
        /// </summary>
        [JsonProperty("nodes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private TNodeConfiguration[] _nodes;

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
        /// This is the backing field for the <see cref="VirtualAddresses"/> property.
        /// </summary>
        [JsonProperty("virtualIps", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancerVirtualAddress[] _virtualIps;

        /// <summary>
        /// This is the backing field for the <see cref="AccessList"/> property.
        /// </summary>
        [JsonProperty("accessList", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NetworkItem[] _accessList;

        /// <summary>
        /// This is the backing field for the <see cref="Algorithm"/> property.
        /// </summary>
        [JsonProperty("algorithm", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancingAlgorithm _algorithm;

        /// <summary>
        /// This is the backing field for the <see cref="ConnectionLogging"/> property.
        /// </summary>
        [JsonProperty("connectionLogging", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancerEnabledFlag _connectionLogging;

        /// <summary>
        /// This is the backing field for the <see cref="ContentCaching"/> property.
        /// </summary>
        [JsonProperty("contentCaching", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancerEnabledFlag _contentCaching;

        /// <summary>
        /// This is the backing field for the <see cref="ConnectionThrottles"/> property.
        /// </summary>
        [JsonProperty("connectionThrottle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ConnectionThrottles _connectionThrottle;

        /// <summary>
        /// This is the backing field for the <see cref="HealthMonitor"/> property.
        /// </summary>
        [JsonProperty("healthMonitor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private JObject _healthMonitor;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancerMetadataItem[] _metadata;

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
        /// This is the backing field for the <see cref="SessionPersistence"/> property.
        /// </summary>
        [JsonProperty("sessionPersistence", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private SessionPersistence _sessionPersistence;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerConfiguration{TNodeConfiguration}"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used during JSON deserialization of derived types.
        /// </remarks>
        [JsonConstructor]
        protected LoadBalancerConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerConfiguration{TNodeConfiguration}"/>
        /// class with the specified values.
        /// </summary>
        /// <param name="name">The name of the load balancer.</param>
        /// <param name="protocol">The load balancing protocol to use for this load balancer.</param>
        /// <param name="virtualAddresses">A collection of <see cref="LoadBalancerVirtualAddress"/> objects describing the virtual addresses to assign to the load balancer.</param>
        /// <param name="nodes">A collection of <typeparamref name="TNodeConfiguration"/> objects describing the nodes in the load balancer. If this value is <see langword="null"/>, the load balancer will be created without any nodes.</param>
        /// <param name="halfClosed"><see langword="true"/> to enable half-closed support for the load balancer; otherwise, <see langword="false"/>. Half-Closed support provides the ability for one end of the connection to terminate its output, while still receiving data from the other end. Only applies to TCP/TCP_CLIENT_FIRST protocols.</param>
        /// <param name="accessList">A collection of <see cref="NetworkItem"/> objects describing the access list for the load balancer. If this value is <see langword="null"/>, the load balancer will be created without an access list configured.</param>
        /// <param name="algorithm">The load balancing algorithm that defines how traffic should be directed between back-end nodes. If this value is <see langword="null"/>, a provider-specific default algorithm will be used.</param>
        /// <param name="connectionLogging"><see langword="true"/> to enable connection logging; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value will be used.</param>
        /// <param name="contentCaching"><see langword="true"/> to enable content caching; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value will be used.</param>
        /// <param name="connectionThrottle">A <see cref="ConnectionThrottles"/> object defining the connection throttling configuration for the load balancer. If this value is <see langword="null"/>, a provider-specific default value will be used.</param>
        /// <param name="healthMonitor">A <see cref="HealthMonitor"/> object defining the health monitor to configure for the load balancer. If this value is <see langword="null"/>, the load balancer will be created with no health monitor configured.</param>
        /// <param name="metadata">A collection of <see cref="LoadBalancerMetadataItem"/> objects defining the initial metadata for the load balancer. If this value is <see langword="null"/>, the load balancer will be created without any initial custom metadata.</param>
        /// <param name="timeout">The timeout value for the load balancer and communications with its nodes. If this value is <see langword="null"/>, a provider-specific default value will be used.</param>
        /// <param name="sessionPersistence">A <see cref="SessionPersistence"/> object defining the session persistence configuration for the load balancer. If this value is <see langword="null"/>, the load balancer will be created with session persistence disabled.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="protocol"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddresses"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="nodes"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddresses"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="virtualAddresses"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="accessList"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any <see langword="null"/> values.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="timeout"/> is negative or <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public LoadBalancerConfiguration(string name, LoadBalancingProtocol protocol, IEnumerable<LoadBalancerVirtualAddress> virtualAddresses, IEnumerable<TNodeConfiguration> nodes = null, bool? halfClosed = null, IEnumerable<NetworkItem> accessList = null, LoadBalancingAlgorithm algorithm = null, bool? connectionLogging = null, bool? contentCaching = null, ConnectionThrottles connectionThrottle = null, HealthMonitor healthMonitor = null, IEnumerable<LoadBalancerMetadataItem> metadata = null, TimeSpan? timeout = null, SessionPersistence sessionPersistence = null)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (protocol == null)
                throw new ArgumentNullException("protocol");
            if (virtualAddresses == null)
                throw new ArgumentNullException("virtualAddresses");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("timeout");

            _name = name;
            _nodes = nodes != null ? nodes.ToArray() : null;
            _protocolName = protocol != null ? protocol.Name : null;
            _port = protocol != null ? (int?)protocol.Port : null;
            _halfClosed = halfClosed;
            _virtualIps = virtualAddresses.ToArray();
            _accessList = accessList != null ? accessList.ToArray() : null;
            _algorithm = algorithm;
            if (connectionLogging.HasValue)
                _connectionLogging = connectionLogging.Value ? LoadBalancerEnabledFlag.True : LoadBalancerEnabledFlag.False;
            if (contentCaching.HasValue)
                _contentCaching = contentCaching.Value ? LoadBalancerEnabledFlag.True : LoadBalancerEnabledFlag.False;
            _connectionThrottle = connectionThrottle;
            _healthMonitor = healthMonitor != null ? JObject.FromObject(healthMonitor) : null;
            _metadata = metadata != null ? metadata.ToArray() : null;
            _timeout = timeout != null ? (int?)timeout.Value.TotalSeconds : null;
            _sessionPersistence = sessionPersistence;

            if (_nodes != null && _nodes.Contains(null))
                throw new ArgumentException("nodes cannot contain any null values", "nodes");
            if (_virtualIps.Contains(null))
                throw new ArgumentException("virtualAddresses cannot contain any null values", "virtualAddresses");
            if (_accessList != null && _accessList.Contains(null))
                throw new ArgumentException("accessList cannot contain any null values", "accessList");
            if (_metadata != null && _metadata.Contains(null))
                throw new ArgumentException("metadata cannot contain any null values", "metadata");
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
        /// Gets a collection of <typeparamref name="TNodeConfiguration"/> objects describing the
        /// nodes associated with the load balancer.
        /// </summary>
        public ReadOnlyCollection<TNodeConfiguration> Nodes
        {
            get
            {
                if (_nodes == null)
                    return null;

                return new ReadOnlyCollection<TNodeConfiguration>(_nodes);
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
        /// Gets a collection of <see cref="LoadBalancerVirtualAddress"/> objects describing
        /// the virtual addresses associated with the load balancer.
        /// </summary>
        public ReadOnlyCollection<LoadBalancerVirtualAddress> VirtualAddresses
        {
            get
            {
                if (_virtualIps == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancerVirtualAddress>(_virtualIps);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="NetworkItem"/> objects describing the access
        /// list for the load balancer.
        /// </summary>
        public ReadOnlyCollection<NetworkItem> AccessList
        {
            get
            {
                if (_accessList == null)
                    return null;

                return new ReadOnlyCollection<NetworkItem>(_accessList);
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
        /// Gets a value indicating whether or not connection logging is enabled for the load balancer.
        /// </summary>
        public bool? ConnectionLogging
        {
            get
            {
                if (_connectionLogging == null)
                    return null;

                return _connectionLogging.Enabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not content caching is enabled for the load balancer.
        /// </summary>
        public bool? ContentCaching
        {
            get
            {
                if (_contentCaching == null)
                    return null;

                return _contentCaching.Enabled;
            }
        }

        /// <summary>
        /// Gets the connection throttling configuration for the load balancer.
        /// </summary>
        public ConnectionThrottles ConnectionThrottles
        {
            get
            {
                return _connectionThrottle;
            }
        }

        /// <summary>
        /// Gets the health monitor configured for the load balancer.
        /// </summary>
        public HealthMonitor HealthMonitor
        {
            get
            {
                if (_healthMonitor == null)
                    return null;

                return HealthMonitor.FromJObject(_healthMonitor);
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="LoadBalancerMetadataItem"/> objects describing
        /// the metadata associated with the load balancer.
        /// </summary>
        public ReadOnlyCollection<LoadBalancerMetadataItem> Metadata
        {
            get
            {
                if (_metadata == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancerMetadataItem>(_metadata);
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

        /// <summary>
        /// Gets the session persistence configuration for the load balancer.
        /// </summary>
        public SessionPersistence SessionPersistence
        {
            get
            {
                return _sessionPersistence;
            }
        }
    }
}
