using System.Collections.ObjectModel;

namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using net.openstack.Core.Domain.Converters;

    using Newtonsoft.Json;

    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the configurable properties of the JSON representation of
    /// an Entity resource in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <seealso cref="Entity"/>
    /// <seealso cref="NewEntityConfiguration"/>
    /// <seealso cref="UpdateEntityConfiguration"/>
    /// <seealso cref="IMonitoringService.CreateEntityAsync"/>
    /// <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-entities.html">Entities (Rackspace Cloud Monitoring Developer Guide - API v1.0)</see>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class EntityConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="AgentId"/> property.
        /// </summary>
        [JsonProperty("agent_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private AgentId _agentId;

        /// <summary>
        /// This is the backing field for the <see cref="Label"/> property.
        /// </summary>
        [JsonProperty("label", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _label;

        /// <summary>
        /// This is the backing field for the <see cref="IPAddresses"/> property.
        /// </summary>
        [JsonProperty("ip_addresses", ItemConverterType = typeof(IPAddressSimpleConverter), DefaultValueHandling = DefaultValueHandling.Ignore)]
        private IDictionary<string, IPAddress> _ipAddresses;

        /// <summary>
        /// This is the backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private IDictionary<string, string> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected EntityConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="label">The name for the entity. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="agentId">The agent which this entity is bound to. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="ipAddresses">The IP addresses which can be referenced by checks on this entity. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <param name="metadata">A collection of metadata to associate with the entity. If this value is <see langword="null"/>, the underlying property will be omitted from the JSON representation of the object.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="label"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="ipAddresses"/> contains any empty keys.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="metadata"/> contains any empty keys.</para>
        /// </exception>
        protected EntityConfiguration(string label, AgentId agentId, IDictionary<string, IPAddress> ipAddresses, IDictionary<string, string> metadata)
        {
            if (label == string.Empty)
                throw new ArgumentException("label cannot be empty");

            _label = label;
            _agentId = agentId;
            _ipAddresses = ipAddresses;
            if (_ipAddresses != null)
            {
                if (_ipAddresses.ContainsKey(string.Empty))
                    throw new ArgumentException("ipAddresses cannot contain any empty keys", "ipAddresses");
            }

            _metadata = metadata;
            if (_metadata != null)
            {
                if (_metadata.ContainsKey(string.Empty))
                    throw new ArgumentException("metadata cannot contain any empty keys", "metadata");
            }
        }

        /// <summary>
        /// Gets the ID of the agent which reports information from this entity.
        /// </summary>
        public AgentId AgentId
        {
            get
            {
                return _agentId;
            }
        }

        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        public string Label
        {
            get
            {
                return _label;
            }
        }

        /// <summary>
        /// Gets a dictionary which maps target aliases to IP addresses associated with the entity.
        /// </summary>
        public ReadOnlyDictionary<string, IPAddress> IPAddresses
        {
            get
            {
                if (_ipAddresses == null)
                    return null;

                return new ReadOnlyDictionary<string, IPAddress>(_ipAddresses);
            }
        }

        /// <summary>
        /// Gets a collection of custom metadata associated with the entity.
        /// </summary>
        public ReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                if (_metadata == null)
                    return null;

                return new ReadOnlyDictionary<string, string>(_metadata);
            }
        }
    }
}
