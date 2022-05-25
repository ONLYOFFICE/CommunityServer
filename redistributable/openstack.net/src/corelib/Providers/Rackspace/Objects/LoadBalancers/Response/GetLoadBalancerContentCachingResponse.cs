namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetLoadBalancerContentCachingResponse
    {
        [JsonProperty("contentCaching")]
        private LoadBalancerEnabledFlag _body;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetLoadBalancerContentCachingResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GetLoadBalancerContentCachingResponse()
        {
        }

        protected GetLoadBalancerContentCachingResponse(bool enabled)
        {
            _body = enabled ? LoadBalancerEnabledFlag.True : LoadBalancerEnabledFlag.False;
        }

        public bool? Enabled
        {
            get
            {
                if (_body == null)
                    return null;

                return _body.Enabled;
            }
        }
    }
}
