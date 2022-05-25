namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;

    using net.openstack.Core;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents the role of a node within a load balancer.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known node conditions,
    /// with added support for unknown conditions returned by a server extension.
    /// </remarks>
    /// <seealso cref="NodeConfiguration.Condition"/>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Modify_Nodes-d1e2503.html">Modify Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NodeCondition.Converter))]
    public sealed class NodeCondition : ExtensibleEnum<NodeCondition>
    {
        private static readonly ConcurrentDictionary<string, NodeCondition> _states =
            new ConcurrentDictionary<string, NodeCondition>(StringComparer.OrdinalIgnoreCase);
        private static readonly NodeCondition _enabled = FromName("ENABLED");
        private static readonly NodeCondition _disabled = FromName("DISABLED");
        private static readonly NodeCondition _draining = FromName("DRAINING");

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCondition"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private NodeCondition(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="NodeCondition"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="NodeCondition"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static NodeCondition FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _states.GetOrAdd(name, i => new NodeCondition(i));
        }

        /// <summary>
        /// Gets a <see cref="NodeCondition"/> instance representing a node which is permitted
        /// to accept new connections.
        /// </summary>
        public static NodeCondition Enabled
        {
            get
            {
                return _enabled;
            }
        }

        /// <summary>
        /// Gets a <see cref="NodeCondition"/> instance representing a node which is not permitted
        /// to accept any new connections regardless of the session persistence configuration.
        /// Existing connections are forcibly terminated.
        /// </summary>
        public static NodeCondition Disabled
        {
            get
            {
                return _disabled;
            }
        }

        /// <summary>
        /// Gets a <see cref="NodeCondition"/> instance representing a node which is allowed to
        /// service existing established connections and connections that are being directed to
        /// it as a result of the session persistence configuration.
        /// </summary>
        public static NodeCondition Draining
        {
            get
            {
                return _draining;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NodeCondition"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NodeCondition FromName(string name)
            {
                return NodeCondition.FromName(name);
            }
        }
    }
}
