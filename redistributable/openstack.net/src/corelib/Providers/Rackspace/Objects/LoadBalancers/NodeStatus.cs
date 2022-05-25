namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents load balancer node status.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known node statuses,
    /// with added support for unknown statuses returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Nodes-d1e2218.html">List Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NodeStatus.Converter))]
    public sealed class NodeStatus : ExtensibleEnum<NodeStatus>
    {
        private static readonly ConcurrentDictionary<string, NodeStatus> _types =
            new ConcurrentDictionary<string, NodeStatus>(StringComparer.OrdinalIgnoreCase);
        private static readonly NodeStatus _online = FromName("ONLINE");

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeStatus"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private NodeStatus(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="NodeStatus"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="NodeStatus"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static NodeStatus FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new NodeStatus(i));
        }

        /// <summary>
        /// Gets a <see cref="NodeStatus"/> representing <placeholder>placeholder</placeholder>.
        /// </summary>
        public static NodeStatus Online
        {
            get
            {
                return _online;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NodeStatus"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NodeStatus FromName(string name)
            {
                return NodeStatus.FromName(name);
            }
        }
    }
}
