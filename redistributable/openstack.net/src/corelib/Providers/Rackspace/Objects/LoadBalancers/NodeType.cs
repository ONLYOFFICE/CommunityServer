namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents load balancer node type.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known node types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Nodes-d1e2379.html">Add Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NodeType.Converter))]
    public sealed class NodeType : ExtensibleEnum<NodeType>
    {
        private static readonly ConcurrentDictionary<string, NodeType> _types =
            new ConcurrentDictionary<string, NodeType>(StringComparer.OrdinalIgnoreCase);
        private static readonly NodeType _primary = FromName("PRIMARY");
        private static readonly NodeType _secondary = FromName("SECONDARY");

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private NodeType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="NodeType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="NodeType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static NodeType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new NodeType(i));
        }

        /// <summary>
        /// Gets a <see cref="NodeType"/> representing a node in the normal rotation to receive traffic from the load balancer.
        /// </summary>
        public static NodeType Primary
        {
            get
            {
                return _primary;
            }
        }

        /// <summary>
        /// Gets a <see cref="NodeType"/> representing a node only in the rotation to receive traffic from the load balancer when all the <see cref="Primary"/> nodes fail.
        /// </summary>
        public static NodeType Secondary
        {
            get
            {
                return _secondary;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NodeType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NodeType FromName(string name)
            {
                return NodeType.FromName(name);
            }
        }
    }
}
