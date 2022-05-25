namespace net.openstack.Core.Domain
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an <see cref="EndpointTemplate"/>.
    /// </summary>
    /// <remarks>
    /// <para>This object is part of the <c>OS-KSCATALOG</c> extension to the OpenStack Identity Service V2.</para>
    /// </remarks>
    /// <seealso cref="EndpointTemplate.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(EndpointTemplateId.Converter))]
    public sealed class EndpointTemplateId : ResourceIdentifier<EndpointTemplateId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointTemplateId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public EndpointTemplateId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="EndpointTemplateId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override EndpointTemplateId FromValue(string id)
            {
                return new EndpointTemplateId(id);
            }
        }
    }
}
