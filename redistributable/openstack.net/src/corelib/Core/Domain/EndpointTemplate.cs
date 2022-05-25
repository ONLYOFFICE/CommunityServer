namespace net.openstack.Core.Domain
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON representation of an endpoint template resource in the OpenStack Identity Service V2.
    /// </summary>
    /// <remarks>
    /// <para>This object is part of the <c>OS-KSCATALOG</c> extension to the OpenStack Identity Service V2.</para>
    /// </remarks>
    /// <seealso href="http://developer.openstack.org/api-ref-identity-v2.html#os-kscatalog-ext">OS-KSCATALOG admin extension (Identity API v2.0 - OpenStack Complete API Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("{Id, nq}")]
    public class EndpointTemplate : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private EndpointTemplateId _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointTemplate"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected EndpointTemplate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointTemplate"/> class with the specified endpoint template
        /// identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the endpoint template.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        public EndpointTemplate(EndpointTemplateId id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            _id = id;
        }

        /// <summary>
        /// Gets the unique identifier for the endpoint template.
        /// </summary>
        /// <value>
        /// <para>The unique identifier for the endpoint template.</para>
        /// <token>NullIfNotIncluded</token>
        /// </value>
        public EndpointTemplateId Id
        {
            get
            {
                return _id;
            }
        }
    }
}
