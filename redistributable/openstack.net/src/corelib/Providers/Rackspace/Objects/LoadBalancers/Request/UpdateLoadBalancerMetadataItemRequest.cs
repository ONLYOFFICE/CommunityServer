namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Request
{
    using System;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class UpdateLoadBalancerMetadataItemRequest
    {
        [JsonProperty("meta")]
        private UpdateMetadataItemRequestBody _body;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateLoadBalancerMetadataItemRequest"/> class
        /// with the specified metadata value.
        /// </summary>
        /// <param name="value">The updated metadata value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
        public UpdateLoadBalancerMetadataItemRequest(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            _body = new UpdateMetadataItemRequestBody(value);
        }

        [JsonObject(MemberSerialization.OptIn)]
        protected class UpdateMetadataItemRequestBody
        {
            [JsonProperty("value")]
            private string _value;

            /// <summary>
            /// Initializes a new instance of the <see cref="UpdateMetadataItemRequestBody"/> class
            /// with the specified metadata value.
            /// </summary>
            /// <param name="value">The updated metadata value.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
            public UpdateMetadataItemRequestBody(string value)
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                _value = value;
            }
        }
    }
}
