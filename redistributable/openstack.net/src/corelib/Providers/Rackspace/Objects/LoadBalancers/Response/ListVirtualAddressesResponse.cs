namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.ObjectModel;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListVirtualAddressesResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        [JsonProperty("virtualIps")]
        private LoadBalancerVirtualAddress[] _virtualAddresses;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListVirtualAddressesResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListVirtualAddressesResponse()
        {
        }

        public ReadOnlyCollection<LoadBalancerVirtualAddress> VirtualAddresses
        {
            get
            {
                if (_virtualAddresses == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancerVirtualAddress>(_virtualAddresses);
            }
        }
    }
}
