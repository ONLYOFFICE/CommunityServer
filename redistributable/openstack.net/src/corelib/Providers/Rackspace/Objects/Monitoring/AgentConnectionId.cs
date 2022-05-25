namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of an agent connection in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="AgentConnection.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AgentConnectionId.Converter))]
    public sealed class AgentConnectionId : ResourceIdentifier<AgentConnectionId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentConnectionId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The agent connection identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public AgentConnectionId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AgentConnectionId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AgentConnectionId FromValue(string id)
            {
                return new AgentConnectionId(id);
            }
        }
    }
}
