namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateLoadBalancerRequest
    {
        [JsonProperty("loadBalancer")]
        private LoadBalancerUpdate _loadBalancerUpdate;

        public UpdateLoadBalancerRequest(LoadBalancerUpdate update)
        {
            _loadBalancerUpdate = update;
        }
    }
}
