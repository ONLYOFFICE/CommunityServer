namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Load Balancing Algorithms request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancing_Algorithms-d1e4459.html">List Load Balancing Algorithms (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListLoadBalancingAlgorithmsResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Algorithms"/> property.
        /// </summary>
        [JsonProperty("algorithms")]
        private SerializedLoadBalancingAlgorithm[] _algorithms;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListLoadBalancingAlgorithmsResponse"/>
        /// class during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListLoadBalancingAlgorithmsResponse()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="LoadBalancingAlgorithm"/> objects
        /// describing the load balancing algorithms supported by a load balancer
        /// service provider.
        /// </summary>
        public IEnumerable<LoadBalancingAlgorithm> Algorithms
        {
            get
            {
                return _algorithms.Select(i => i.Algorithm);
            }
        }

        /// <summary>
        /// This models the intermediate JSON representation of a named object.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        private class SerializedLoadBalancingAlgorithm
        {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
            /// <summary>
            /// This is the backing field for the <see cref="Algorithm"/> property.
            /// </summary>
            [JsonProperty("name")]
            private LoadBalancingAlgorithm _name;
#pragma warning restore 649

            /// <summary>
            /// Gets the load balancing algorithm.
            /// </summary>
            public LoadBalancingAlgorithm Algorithm
            {
                get
                {
                    return _name;
                }
            }
        }
    }
}
