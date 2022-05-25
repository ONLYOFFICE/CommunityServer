namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetLoadBalancerErrorPageResponse
    {
        [JsonProperty("errorpage")]
        private ErrorPageBody _body;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetLoadBalancerErrorPageResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GetLoadBalancerErrorPageResponse()
        {
        }

        public GetLoadBalancerErrorPageResponse(string content)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("content cannot be empty");

            _body = new ErrorPageBody(content);
        }

        public string Content
        {
            get
            {
                if (_body == null)
                    return null;

                return _body.Content;
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        protected class ErrorPageBody
        {
            [JsonProperty("content")]
            private string _content;

            [JsonConstructor]
            protected ErrorPageBody()
            {
            }

            public ErrorPageBody(string content)
            {
                if (content == null)
                    throw new ArgumentNullException("content");
                if (string.IsNullOrEmpty(content))
                    throw new ArgumentException("content cannot be empty");

                _content = content;
            }

            public string Content
            {
                get
                {
                    return _content;
                }
            }
        }
    }
}
