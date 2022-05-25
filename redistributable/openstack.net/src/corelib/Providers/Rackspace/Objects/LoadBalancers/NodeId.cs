namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a load balancer node in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <seealso cref="Node.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(NodeId.Converter))]
    public sealed class NodeId : ResourceIdentifier<NodeId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The load balancer node identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public NodeId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="NodeId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override NodeId FromValue(string id)
            {
                return new NodeId(id);
            }
        }
    }
}
