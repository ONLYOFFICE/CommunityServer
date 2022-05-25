namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a load balancer node in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Node : NodeConfiguration
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private NodeId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private NodeStatus _status;
#pragma warning restore 649
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Node()
        {
        }

        /// <summary>
        /// Gets unique ID representing this node within the load balancers service.
        /// </summary>
        /// <value>
        /// The unique ID for the load balancer node, or <see langword="null"/> if the JSON response
        /// from the server did not include this property.
        /// </value>
        public NodeId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the status of the load balancer node.
        /// </summary>
        /// <value>
        /// The status of the load balancer node, or <see langword="null"/> if the JSON response
        /// from the server did not include this property.
        /// </value>
        public NodeStatus Status
        {
            get
            {
                return _status;
            }
        }
    }
}
