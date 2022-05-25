namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Usage request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Usage-d1e3014.html">List Usage (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListLoadBalancerUsageResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="UsageRecords"/> property.
        /// </summary>
        [JsonProperty("loadBalancerUsageRecords")]
        private LoadBalancerUsage[] _loadBalancerUsage;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListLoadBalancerUsageResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListLoadBalancerUsageResponse()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="LoadBalancerUsage"/> objects describing the load
        /// balancer usage resources in a load balancer service provider.
        /// </summary>
        public ReadOnlyCollection<LoadBalancerUsage> UsageRecords
        {
            get
            {
                if (_loadBalancerUsage == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancerUsage>(_loadBalancerUsage);
            }
        }
    }
}
