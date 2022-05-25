namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class SetLoadBalancerContentCachingRequest : GetLoadBalancerContentCachingResponse
    {
        public SetLoadBalancerContentCachingRequest(bool enabled)
            : base(enabled)
        {
        }
    }
}
