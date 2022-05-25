namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This object models the JSON representation of a configuration update to apply
    /// to an existing load balancer node.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NodeUpdate : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Condition"/> property.
        /// </summary>
        [JsonProperty("condition", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NodeCondition _condition;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private NodeType _type;

        /// <summary>
        /// This is the backing field for the <see cref="Weight"/> property.
        /// </summary>
        [JsonProperty("weight", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _weight;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeUpdate"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NodeUpdate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeUpdate"/> class.
        /// </summary>
        /// <param name="condition">The condition for the node, which determines its role within the load balancer. If this value is <see langword="null"/>, the existing value for the node is not changed.</param>
        /// <param name="type">The node type. If this value is <see langword="null"/>, a provider-specific default value will be used. If this value is <see langword="null"/>, the existing value for the node is not changed.</param>
        /// <param name="weight">The weight of the node. If this value is <see langword="null"/>, the existing value for the node is not changed.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="weight"/> is less than or equal to 0.</exception>
        public NodeUpdate(NodeCondition condition = null, NodeType type = null, int? weight = null)
        {
            if (weight <= 0)
                throw new ArgumentOutOfRangeException("weight");

            _condition = condition;
            _type = type;
            _weight = weight;
        }

        /// <summary>
        /// Gets the condition for the node, which determines its role within the load balancer.
        /// </summary>
        public NodeCondition Condition
        {
            get
            {
                return _condition;
            }
        }

        /// <summary>
        /// Gets the load balancer node type.
        /// </summary>
        public NodeType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the weight of the load balancer node.
        /// </summary>
        /// <remarks>
        /// This property is only used by load balancers with a weighted algorithm, such as
        /// <see cref="LoadBalancingAlgorithm.WeightedRoundRobin"/>.
        /// </remarks>
        public int? Weight
        {
            get
            {
                return _weight;
            }
        }
    }
}
