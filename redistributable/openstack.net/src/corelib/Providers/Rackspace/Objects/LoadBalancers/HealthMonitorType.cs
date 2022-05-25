namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents load balancer health monitor type.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known monitor types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Monitors-d1e3370.html">Monitors (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(HealthMonitorType.Converter))]
    public sealed class HealthMonitorType : ExtensibleEnum<HealthMonitorType>
    {
        private static readonly ConcurrentDictionary<string, HealthMonitorType> _types =
            new ConcurrentDictionary<string, HealthMonitorType>(StringComparer.OrdinalIgnoreCase);
        private static readonly HealthMonitorType _connect = FromName("CONNECT");
        private static readonly HealthMonitorType _http = FromName("HTTP");
        private static readonly HealthMonitorType _https = FromName("HTTPS");

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthMonitorType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private HealthMonitorType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="HealthMonitorType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="HealthMonitorType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static HealthMonitorType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new HealthMonitorType(i));
        }

        /// <summary>
        /// Gets a <see cref="HealthMonitorType"/> representing a connect monitor.
        /// </summary>
        public static HealthMonitorType Connect
        {
            get
            {
                return _connect;
            }
        }

        /// <summary>
        /// Gets a <see cref="HealthMonitorType"/> representing an HTTP monitor.
        /// </summary>
        public static HealthMonitorType Http
        {
            get
            {
                return _http;
            }
        }

        /// <summary>
        /// Gets a <see cref="HealthMonitorType"/> representing an HTTPS monitor.
        /// </summary>
        public static HealthMonitorType Https
        {
            get
            {
                return _https;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="HealthMonitorType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override HealthMonitorType FromName(string name)
            {
                return HealthMonitorType.FromName(name);
            }
        }
    }
}
