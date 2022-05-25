namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;
    using LoadBalancer = net.openstack.Providers.Rackspace.Objects.LoadBalancers.LoadBalancer;
    using LoadBalancerId = net.openstack.Providers.Rackspace.Objects.LoadBalancers.LoadBalancerId;

    /// <summary>
    /// This class models the JSON representation of a load balancer to add new
    /// servers created by a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="ServerLaunchArguments.LoadBalancers"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerArgument : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="LoadBalancerId"/> property.
        /// </summary>
        [JsonProperty("loadBalancerId")]
        private LoadBalancerId _loadBalancerId;

        /// <summary>
        /// This is the backing field for the <see cref="Port"/> property.
        /// </summary>
        [JsonProperty("port")]
        private int? _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerArgument"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerArgument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerArgument"/> class
        /// with the specified load balancer ID and port.
        /// </summary>
        /// <param name="loadBalancerId">The ID of the load balancer to add new servers to. See <see cref="LoadBalancer.Id"/>.</param>
        /// <param name="port">The port used for the load balancer protocol. See <see cref="LoadBalancers.LoadBalancerConfiguration{TNodeConfiguration}.Port"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="loadBalancerId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than 0 or greater than 65535.</exception>
        public LoadBalancerArgument(LoadBalancerId loadBalancerId, int port)
        {
            if (loadBalancerId == null)
                throw new ArgumentNullException("loadBalancerId");
            if (port <= 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");

            _loadBalancerId = loadBalancerId;
            _port = port;
        }

        /// <summary>
        /// Gets the ID of the load balancer to add new servers to.
        /// </summary>
        /// <seealso cref="LoadBalancer.Id"/>
        public LoadBalancerId LoadBalancerId
        {
            get
            {
                return _loadBalancerId;
            }
        }

        /// <summary>
        /// Gets the port on which new servers will receive traffic from the load balancer, often port 80.
        /// </summary>
        /// <seealso cref="LoadBalancers.LoadBalancerConfiguration{TNodeConfiguration}.Port"/>
        public int? Port
        {
            get
            {
                return _port;
            }
        }
    }
}
