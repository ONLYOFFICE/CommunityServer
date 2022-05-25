namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an agent token in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="AgentToken.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AgentTokenId.Converter))]
    public sealed class AgentTokenId : ResourceIdentifier<AgentTokenId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTokenId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The agent token identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AgentTokenId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AgentTokenId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AgentTokenId FromValue(string id)
            {
                return new AgentTokenId(id);
            }
        }
    }
}
