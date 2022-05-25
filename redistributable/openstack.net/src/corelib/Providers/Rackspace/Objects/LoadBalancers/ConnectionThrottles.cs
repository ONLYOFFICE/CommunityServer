namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a connection throttling configuration for a load
    /// balancer in the load balancer service.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ConnectionThrottles : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="MaxConnectionRate"/> property.
        /// </summary>
        [JsonProperty("maxConnectionRate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _maxConnectionRate;

        /// <summary>
        /// This is the backing field for the <see cref="MaxConnections"/> property.
        /// </summary>
        [JsonProperty("maxConnections", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _maxConnections;

        /// <summary>
        /// This is the backing field for the <see cref="MinConnections"/> property.
        /// </summary>
        [JsonProperty("minConnections", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _minConnections;

        /// <summary>
        /// This is the backing field for the <see cref="RateInterval"/> property.
        /// </summary>
        [JsonProperty("rateInterval", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _rateInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionThrottles"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ConnectionThrottles()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionThrottles"/> class with
        /// the specified configuration.
        /// </summary>
        /// <param name="maxConnectionRate">The maximum number of connections per rate interval to allow from a single IP address, or <c>0</c> to not limit the connection rate. If this value is <see langword="null"/>, the value for this throttle will not be changed during a call to <see cref="ILoadBalancerService.UpdateThrottlesAsync"/>.</param>
        /// <param name="maxConnections">The maximum number of connections to allow from a single IP address, or <c>0</c> to not limit the number connections. If this value is <see langword="null"/>, the value for this throttle will not be changed during a call to <see cref="ILoadBalancerService.UpdateThrottlesAsync"/>.</param>
        /// <param name="minConnections">The minimum number of connections to allow from an IP address before applying throttling restrictions. If this value is <see langword="null"/>, the value for this throttle will not be changed during a call to <see cref="ILoadBalancerService.UpdateThrottlesAsync"/>.</param>
        /// <param name="rateInterval">The time period for which the connection rate limit is evaluated. If this value is <see langword="null"/>, the value for this throttle will not be changed during a call to <see cref="ILoadBalancerService.UpdateThrottlesAsync"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="maxConnectionRate"/> is less than 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="maxConnections"/> is less than 0.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="minConnections"/> is less than 0.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="rateInterval"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        public ConnectionThrottles(int? maxConnectionRate, int? maxConnections, int? minConnections, TimeSpan? rateInterval)
        {
            if (maxConnectionRate < 0)
                throw new ArgumentOutOfRangeException("maxConnectionRate");
            if (maxConnections < 0)
                throw new ArgumentOutOfRangeException("maxConnections");
            if (minConnections < 0)
                throw new ArgumentOutOfRangeException("minConnections");
            if (rateInterval <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("rateInterval");

            _maxConnectionRate = maxConnectionRate;
            _maxConnections = maxConnections;
            _minConnections = minConnections;
            _rateInterval = rateInterval != null ? (int?)rateInterval.Value.TotalSeconds : default(int?);
        }

        /// <summary>
        /// Gets the maximum number of connections allowed from a single IP address in the defined <see cref="RateInterval"/>.
        /// A value of 0 indicates an unlimited connection rate. A value of <see langword="null"/> indicates this connection limit is
        /// not configured.
        /// </summary>
        /// <value>
        /// The maximum number of connections allowed from a single IP address in the defined <see cref="RateInterval"/>,
        /// or one of the following values:
        ///
        /// <list type="bullet">
        /// <item>0, if the connection rate is configured but unlimited.</item>
        /// <item><see langword="null"/>, if the connection rate limit is not configured.</item>
        /// </list>
        /// </value>
        public int? MaxConnectionRate
        {
            get
            {
                return _maxConnectionRate;
            }
        }

        /// <summary>
        /// Gets the maximum number of connections allowed for a single IP address. A value of 0 indicates
        /// an unlimited number of connections. A value of <see langword="null"/> indicates this connection limit is
        /// not configured.
        /// </summary>
        public int? MaxConnections
        {
            get
            {
                return _maxConnections;
            }
        }

        /// <summary>
        /// Gets the minimum number of connections to allow from an IP address before applying throttling restrictions.
        /// A value of <see langword="null"/> indicates this connection limit is not configured.
        /// </summary>
        public int? MinConnections
        {
            get
            {
                return _minConnections;
            }
        }

        /// <summary>
        /// Gets the time period for which <see cref="MaxConnectionRate"/> is evaluated. For example, a <see cref="MaxConnectionRate"/>
        /// of 30 with a <see cref="RateInterval"/> of 60 seconds would allow a maximum of 30 connections per minute from a single IP
        /// address. A value of <see langword="null"/> indicates this connection limit is not configured.
        /// </summary>
        public TimeSpan? RateInterval
        {
            get
            {
                if (_rateInterval == null)
                    return null;

                return TimeSpan.FromSeconds(_rateInterval.Value);
            }
        }
    }
}
