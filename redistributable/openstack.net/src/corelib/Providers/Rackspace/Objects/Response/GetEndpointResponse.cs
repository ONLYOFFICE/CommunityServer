namespace net.openstack.Providers.Rackspace.Objects.Response
{
    using Newtonsoft.Json;
    using net.openstack.Core.Domain;

    /// <summary>
    /// This class models the JSON representation of the response to the <strong>Get endpoint</strong> HTTP API call in
    /// the OpenStack Identity Service V2.
    /// </summary>
    /// <remarks>
    /// <para>This call is part of the <c>OS-KSCATALOG</c> extension to the OpenStack Identity Service V2.</para>
    /// </remarks>
    /// <seealso href="http://developer.openstack.org/api-ref-identity-v2.html#os-kscatalog-ext">OS-KSCATALOG admin extension (Identity API v2.0 - OpenStack Complete API Reference)</seealso>
    /// <seealso cref="ExtendedEndpoint"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class GetEndpointResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Endpoint"/> property.
        /// </summary>
        [JsonProperty("endpoint", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ExtendedEndpoint _endpoint;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEndpointResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected GetEndpointResponse()
        {
        }

        /// <summary>
        /// Gets additional information about the endpoint.
        /// </summary>
        /// <value>
        /// <para>An <see cref="ExtendedEndpoint"/> object containing the details of the endpoint.</para>
        /// <token>NullIfNotIncluded</token>
        /// </value>
        public ExtendedEndpoint Endpoint
        {
            get
            {
                return _endpoint;
            }
        }
    }
}
