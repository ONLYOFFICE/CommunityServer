namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Nodes and Add Nodes requests.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Nodes-d1e2218.html">List Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Nodes-d1e2379.html">Add Nodes (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListLoadBalancerNodesResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Nodes"/> property.
        /// </summary>
        [JsonProperty("nodes")]
        private Node[] _nodes;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListLoadBalancerNodesResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListLoadBalancerNodesResponse()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="Node"/> objects describing the load balancer
        /// node resources available for a load balancer in the load balancer service.
        /// </summary>
        public ReadOnlyCollection<Node> Nodes
        {
            get
            {
                if (_nodes == null)
                    return null;

                return new ReadOnlyCollection<Node>(_nodes);
            }
        }
    }
}
