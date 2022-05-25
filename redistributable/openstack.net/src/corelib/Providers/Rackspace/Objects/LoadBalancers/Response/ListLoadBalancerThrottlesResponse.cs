namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the Throttle Connections request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Throttle_Connections-d1e4057.html">Throttle Connections (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListLoadBalancerThrottlesResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Throttles"/> property.
        /// </summary>
        [JsonProperty("connectionThrottle")]
        private ConnectionThrottles _throttles;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListLoadBalancerThrottlesResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListLoadBalancerThrottlesResponse()
        {
        }

        /// <summary>
        /// Gets a <see cref="ConnectionThrottles"/> object describing the connection throttling
        /// configuration for a load balancer.
        /// </summary>
        public ConnectionThrottles Throttles
        {
            get
            {
                return _throttles;
            }
        }
    }
}
