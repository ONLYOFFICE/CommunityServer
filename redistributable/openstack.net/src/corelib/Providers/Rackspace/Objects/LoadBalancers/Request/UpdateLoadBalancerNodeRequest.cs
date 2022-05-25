namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using System;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateLoadBalancerNodeRequest
    {
        /// <summary>
        /// This is the backing field for the <see cref="NodeUpdate"/> property.
        /// </summary>
        [JsonProperty("node")]
        private NodeUpdate _nodeUpdate;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateLoadBalancerNodeRequest"/> class
        /// with the specified configuration.
        /// </summary>
        /// <param name="nodeUpdate">The updated configuration for the load balancer node.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="nodeUpdate"/> is <see langword="null"/>.</exception>
        public UpdateLoadBalancerNodeRequest(NodeUpdate nodeUpdate)
        {
            if (nodeUpdate == null)
                throw new ArgumentNullException("nodeUpdate");

            _nodeUpdate = nodeUpdate;
        }

        /// <summary>
        /// Gets the updated configuration for the load balancer node.
        /// </summary>
        public NodeUpdate NodeUpdate
        {
            get
            {
                return _nodeUpdate;
            }
        }
    }
}
