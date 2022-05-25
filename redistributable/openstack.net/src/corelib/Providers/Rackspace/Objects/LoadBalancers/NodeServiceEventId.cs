namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a node service event in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <seealso cref="NodeServiceEvent.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NodeServiceEventId.Converter))]
    public sealed class NodeServiceEventId : ResourceIdentifier<NodeServiceEventId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeServiceEventId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The node service event identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public NodeServiceEventId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NodeServiceEventId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NodeServiceEventId FromValue(string id)
            {
                return new NodeServiceEventId(id);
            }
        }
    }
}
