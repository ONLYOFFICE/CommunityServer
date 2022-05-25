namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using IPAddress = System.Net.IPAddress;

    /// <summary>
    /// This class models the JSON representation of a request to update the properties
    /// of an <see cref="Entity"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.UpdateEntityAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateEntityConfiguration : EntityConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateEntityConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UpdateEntityConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateEntityConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="label">The name for the entity. If this parameter is <see langword="null"/>, the name for the entity is not changed.</param>
        /// <param name="agentId">The agent which this entity is bound to. If this parameter is <see langword="null"/>, the agent for the entity is not changed.</param>
        /// <param name="ipAddresses">The IP addresses which can be referenced by checks on this entity. If this parameter is <see langword="null"/>, the IP addresses for the entity are not changed.</param>
        /// <param name="metadata">A collection of metadata to associate with the entity. If this parameter is <see langword="null"/>, the metadata for the entity is not changed.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="ipAddresses"/> contains any empty keys, or any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys, or any <see langword="null"/> values.</para>
        /// </exception>
        public UpdateEntityConfiguration(string label = null, AgentId agentId = null, IDictionary<string, IPAddress> ipAddresses = null, IDictionary<string, string> metadata = null)
            : base(label, agentId, ipAddresses, metadata)
        {
        }
    }
}
