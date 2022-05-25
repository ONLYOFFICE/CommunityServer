namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a request to perform a traceroute
    /// from a monitoring zone in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.PerformTraceRouteFromMonitoringZoneAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class TraceRouteConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Target"/> property.
        /// </summary>
        [JsonProperty("target")]
        private string _target;

        /// <summary>
        /// This is the backing field for the <see cref="ResolverType"/> property.
        /// </summary>
        [JsonProperty("target_resolver")]
        private TargetResolverType _targetResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRouteConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected TraceRouteConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRouteConfiguration"/> class
        /// with the specified target address.
        /// </summary>
        /// <param name="target">The IP address to target for the traceroute operation.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">If the <see cref="IPAddress.AddressFamily"/> of <paramref name="target"/> is not <see cref="AddressFamily.InterNetwork"/> or <see cref="AddressFamily.InterNetworkV6"/>.</exception>
        public TraceRouteConfiguration(IPAddress target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (target.AddressFamily != AddressFamily.InterNetwork && target.AddressFamily != AddressFamily.InterNetworkV6)
                throw new NotSupportedException("The family of the target address is not supported.");

            _target = target.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRouteConfiguration"/> class
        /// with the specified target and resolver type.
        /// </summary>
        /// <param name="target">The target for the traceroute operation.</param>
        /// <param name="resolverType">The type of resolver to use for resolving the target to an IP address.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="target"/> is empty.</exception>
        public TraceRouteConfiguration(string target, TargetResolverType resolverType)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (string.IsNullOrEmpty(target))
                throw new ArgumentException("target cannot be empty");

            _target = target;
            _targetResolver = resolverType;
        }

        /// <summary>
        /// Gets the target of the traceroute operation.
        /// </summary>
        public string Target
        {
            get
            {
                return _target;
            }
        }

        /// <summary>
        /// Gets the type of resolver to use for resolving the target to an IP address.
        /// </summary>
        public TargetResolverType ResolverType
        {
            get
            {
                return _targetResolver;
            }
        }
    }
}
