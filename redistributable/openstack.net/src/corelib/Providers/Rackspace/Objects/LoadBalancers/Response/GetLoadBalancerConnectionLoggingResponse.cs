namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetLoadBalancerConnectionLoggingResponse
    {
        [JsonProperty("connectionLogging")]
        private LoadBalancerEnabledFlag _body;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetLoadBalancerConnectionLoggingResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GetLoadBalancerConnectionLoggingResponse()
        {
        }

        protected GetLoadBalancerConnectionLoggingResponse(bool enabled)
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
