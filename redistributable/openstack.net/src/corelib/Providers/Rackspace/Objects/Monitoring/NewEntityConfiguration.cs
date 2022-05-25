namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using IPAddress = System.Net.IPAddress;

    /// <summary>
    /// This class models the JSON representation of a request to create a new
    /// <see cref="Entity"/> resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="IMonitoringService.CreateEntityAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NewEntityConfiguration : EntityConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewEntityConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NewEntityConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewEntityConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="label">The name for the entity.</param>
        /// <param name="agentId">The agent which this entity is bound to. If this parameter is <see langword="null"/>, <placeholder>placeholder</placeholder>.</param>
        /// <param name="ipAddresses">The IP addresses which can be referenced by checks on this entity. If this parameter is <see langword="null"/>, <placeholder>placeholder</placeholder>.</param>
        /// <param name="metadata">A collection of metadata to associate with the entity. If this parameter is <see langword="null"/>, the entity is created without any custom metadata.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="label"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="ipAddresses"/> contains any empty keys.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        public NewEntityConfiguration(string label, AgentId agentId, IDictionary<string, IPAddress> ipAddresses, IDictionary<string, string> metadata)
            : base(label, agentId, ipAddresses, metadata)
        {
            if (label == null)
                throw new ArgumentNullException("label");
        }
    }
}
