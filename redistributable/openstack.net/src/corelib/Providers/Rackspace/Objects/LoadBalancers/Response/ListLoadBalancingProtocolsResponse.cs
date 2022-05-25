namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON response used for the List Load Balancing Protocols request.
    /// </summary>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancing_Protocols-d1e4269.html">List Load Balancing Protocols (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListLoadBalancingProtocolsResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Protocols"/> property.
        /// </summary>
        [JsonProperty("protocols")]
        private LoadBalancingProtocol[] _protocols;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListLoadBalancingProtocolsResponse"/>
        /// class during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListLoadBalancingProtocolsResponse()
        {
        }

        /// <summary>
        /// Gets a collection of <see cref="LoadBalancingProtocol"/> objects describing the
        /// protocols supported by the a load balancing service provider.
        /// </summary>
        public ReadOnlyCollection<LoadBalancingProtocol> Protocols
        {
            get
            {
                if (_protocols == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancingProtocol>(_protocols);
            }
        }
    }
}
