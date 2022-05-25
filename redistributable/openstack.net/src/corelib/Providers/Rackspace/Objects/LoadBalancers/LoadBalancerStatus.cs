namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the current status of a load balancer.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known load balancer
    /// statuses, with added support for unknown statuses returned by a server extension.
    /// </remarks>
    /// <seealso cref="LoadBalancer.Status"/>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Update_Load_Balancer_Attributes-d1e1812.html">Update Load Balancer Attributes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(LoadBalancerStatus.Converter))]
    public sealed class LoadBalancerStatus : ExtensibleEnum<LoadBalancerStatus>
    {
        private static readonly ConcurrentDictionary<string, LoadBalancerStatus> _types =
            new ConcurrentDictionary<string, LoadBalancerStatus>(StringComparer.OrdinalIgnoreCase);
        private static readonly LoadBalancerStatus _active = FromName("ACTIVE");
        private static readonly LoadBalancerStatus _build = FromName("BUILD");
        private static readonly LoadBalancerStatus _pendingUpdate = FromName("PENDING_UPDATE");
        private static readonly LoadBalancerStatus _pendingDelete = FromName("PENDING_DELETE");
        private static readonly LoadBalancerStatus _suspended = FromName("SUSPENDED");
        private static readonly LoadBalancerStatus _error = FromName("ERROR");
        private static readonly LoadBalancerStatus _deleted = FromName("DELETED");

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerStatus"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private LoadBalancerStatus(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="LoadBalancerStatus"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="LoadBalancerStatus"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static LoadBalancerStatus FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new LoadBalancerStatus(i));
        }

        /// <summary>
        /// Gets a <see cref="LoadBalancerStatus"/> representing a load balancer that is configured
        /// properly and ready to serve traffic to incoming requests via the configured virtual IPs.
        /// </summary>
        public static LoadBalancerStatus Active
        {
            get
            {
                return _active;
            }
        }

        /// <summary>
        /// Gets a <see cref="LoadBalancerStatus"/> representing a load balancer that is being
        /// provisioned for the first time and configuration is being applied to bring the service
        /// online. The service will not yet be ready to serve incoming requests.
        /// </summary>
        public static LoadBalancerStatus Build
        {
            get
            {
                return _build;
            }
        }

        /// <summary>
        /// Gets a <see cref="LoadBalancerStatus"/> representing a load balancer that is online
        /// but configuration changes are being applied to update the service based on a previous
        /// request.
        /// </summary>
        public static LoadBalancerStatus PendingUpdate
        {
            get
            {
                return _pendingUpdate;
            }
        }

        /// <summary>
        /// Gets a <see cref="LoadBalancerStatus"/> representing a load balancer that is online
        /// but configuration changes are being applied to begin deletion of the service based
        /// on a previous request.
        /// </summary>
        public static LoadBalancerStatus PendingDelete
        {
            get
            {
                return _pendingDelete;
            }
        }

        /// <summary>
        /// Gets a <see cref="LoadBalancerStatus"/> representing a load balancer that has been
        /// taken offline and disabled.
        /// </summary>
        public static LoadBalancerStatus Suspended
        {
            get
            {
                return _suspended;
            }
        }

        /// <summary>
        /// Gets a <see cref="LoadBalancerStatus"/> indicating the system encountered an error
        /// when attempting to configure the load balancer.
        /// </summary>
        public static LoadBalancerStatus Error
        {
            get
            {
                return _error;
            }
        }

        /// <summary>
        /// Gets a <see cref="LoadBalancerStatus"/> representing a load balancer that has been
        /// deleted.
        /// </summary>
        public static LoadBalancerStatus Deleted
        {
            get
            {
                return _deleted;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="LoadBalancerStatus"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override LoadBalancerStatus FromName(string name)
            {
                return LoadBalancerStatus.FromName(name);
            }
        }
    }
}
