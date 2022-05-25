namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Load Balancers request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancers-d1e1367.html">List Load Balancers (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListLoadBalancersResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="LoadBalancers"/> property.
        /// </summary>
        [JsonProperty("loadBalancers")]
        private LoadBalancer[] _loadBalancers;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListLoadBalancersResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListLoadBalancersResponse()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="LoadBalancer"/> objects describing the load
        /// balancer resources in a load balancer service provider.
        /// </summary>
        public ReadOnlyCollection<LoadBalancer> LoadBalancers
        {
            get
            {
                if (_loadBalancers == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancer>(_loadBalancers);
            }
        }
    }
}
