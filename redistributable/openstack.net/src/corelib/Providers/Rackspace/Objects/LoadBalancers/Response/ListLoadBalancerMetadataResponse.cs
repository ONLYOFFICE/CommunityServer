namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Metadata and Add Metadata requests.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Metadata-d1e2218.html">List Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/Add_Metadata-d1e2379.html">Add Metadata (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListLoadBalancerMetadataResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata")]
        private LoadBalancerMetadataItem[] _metadata;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListLoadBalancerMetadataResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListLoadBalancerMetadataResponse()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="LoadBalancerMetadataItem"/> objects describing the
        /// metadata associated with a resource in the load balancer service.
        /// </summary>
        public ReadOnlyCollection<LoadBalancerMetadataItem> Metadata
        {
            get
            {
                if (_metadata == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancerMetadataItem>(_metadata);
            }
        }
    }
}
