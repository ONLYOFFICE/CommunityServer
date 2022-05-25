namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a node service event type.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known event types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso cref="NodeServiceEvent.Type"/>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Node-Events-d1e264.html">View Node Service Events (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NodeServiceEventType.Converter))]
    public sealed class NodeServiceEventType : ExtensibleEnum<NodeServiceEventType>
    {
        private static readonly ConcurrentDictionary<string, NodeServiceEventType> _types =
            new ConcurrentDictionary<string, NodeServiceEventType>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeServiceEventType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private NodeServiceEventType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="NodeServiceEventType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="NodeServiceEventType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static NodeServiceEventType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new NodeServiceEventType(i));
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NodeServiceEventType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NodeServiceEventType FromName(string name)
            {
                return NodeServiceEventType.FromName(name);
            }
        }
    }
}
