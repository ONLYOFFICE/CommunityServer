namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using System;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class CreateLoadBalancerRequest
    {
        [JsonProperty("loadBalancer")]
        private LoadBalancerConfiguration _configuration;

        public CreateLoadBalancerRequest(LoadBalancerConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _configuration = configuration;
        }
    }
}
