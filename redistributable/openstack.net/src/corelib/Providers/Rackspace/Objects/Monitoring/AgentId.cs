namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an agent in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Agent.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AgentId.Converter))]
    public sealed class AgentId : ResourceIdentifier<AgentId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The agent identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AgentId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AgentId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AgentId FromValue(string id)
            {
                return new AgentId(id);
            }
        }
    }
}
