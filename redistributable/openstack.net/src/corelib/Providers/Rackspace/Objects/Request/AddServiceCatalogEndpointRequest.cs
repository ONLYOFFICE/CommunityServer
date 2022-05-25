namespace net.openstack.Providers.Rackspace.Objects.Request
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON request used for the <strong>Add endpoint</strong> HTTP API request in the OpenStack
    /// Identity Service V2.
    /// </summary>
    /// <remarks>
    /// <para>This object is part of the <c>OS-KSCATALOG</c> extension to the OpenStack Identity Service V2.</para>
    /// </remarks>
    /// <seealso href="http://developer.openstack.org/api-ref-identity-v2.html#os-kscatalog-ext">OS-KSCATALOG admin extension (Identity API v2.0 - OpenStack Complete API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class AddServiceCatalogEndpointRequest
    {
        /// <summary>
        /// This is the backing field for the <see cref="EndpointTemplateId"/> property.
        /// </summary>
        /// <remarks>
        /// <para>This API call wraps the endpoint template identifier inside an "EndpointTemplateWithOnlyId"
        /// resource.</para>
        /// </remarks>
        [JsonProperty("OS-KSCATALOG:endpointTemplate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private EndpointTemplate _endpointTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddServiceCatalogEndpointRequest"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected AddServiceCatalogEndpointRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddServiceCatalogEndpointRequest"/> class with the specified
        /// endpoint template identifier.
        /// </summary>
        /// <param name="endpointTemplateId">
        /// The unique identifier of the endpoint template to use for the endpoint.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="endpointTemplateId"/> is <see langword="null"/>.
        /// </exception>
        public AddServiceCatalogEndpointRequest(EndpointTemplateId endpointTemplateId)
        {
            if (endpointTemplateId == null)
                throw new ArgumentNullException("endpointTemplateId");

            _endpointTemplate = new EndpointTemplate(endpointTemplateId);
        }

        /// <summary>
        /// Gets the unique identifier of the endpoint template to use when creating the endpoint.
        /// </summary>
        /// <value>
        /// <para>The unique identifier of the endpoint template to use when creating the endpoint.</para>
        /// <token>NullIfNotIncluded</token>
        /// </value>
        public EndpointTemplateId EndpointTemplateId
        {
            get
            {
                if (_endpointTemplate == null)
                    return null;

                return _endpointTemplate.Id;
            }
        }
    }
}
